using System;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Web.Configuration;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web;

/// <summary>
/// <para>Register Vostok middlewares and their configuration in DI container.</para>
/// <para>Use returning <see cref="IVostokMiddlewaresConfigurator"/> to configure them.</para>
/// <para>Applies reasonable Kestrel defaults.</para>
/// </summary>
public static class AddVostokMiddlewaresExtensions
{
    /// <inheritdoc cref="AddVostokMiddlewaresExtensions"/>
    public static IVostokMiddlewaresConfigurator AddVostokMiddlewares(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .ConfigureKestrelDefaults()
            .AddRequestTracking();

        serviceCollection
            .AddVostokHttpContextTweaks(_ => {})
            .AddVostokRequestInfo(_ => {})
            .AddVostokDistributedContext(_ => {})
            .AddVostokTracing(_ => {})
            .AddThrottling()
            .AddVostokRequestLogging(_ => {})
            .AddVostokDatacenterAwareness(_ => {})
            .AddVostokUnhandledExceptions(_ => {})
            .AddVostokPingApi(_ => {})
            .AddVostokDiagnosticApi(_ => {});

        serviceCollection.AddOptions<TracingSettings>()
            .Configure<IServiceBeacon>((settings, beacon) =>
            {
                if (beacon.ReplicaInfo.TryGetUrl(out var url))
                    settings.BaseUrl = url;
            });

        serviceCollection.AddOptions<PingApiSettings>()
            .Configure<IVostokHostingEnvironment, InitializedFlag>((settings, environment, initFlag) =>
            {
                settings.CommitHashProvider = () => AssemblyCommitHashExtractor.ExtractFromAssembly(Assembly.GetEntryAssembly()!);
                settings.InitializationCheck = () => initFlag.Value;

                if (environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
                    settings.HealthCheck = () => diagnostics.HealthTracker.CurrentStatus == HealthStatus.Healthy;
            });

        return new VostokMiddlewaresConfigurator(serviceCollection);
    }

    private static IServiceCollection ConfigureKestrelDefaults(this IServiceCollection serviceCollection)
    {
        return serviceCollection.Configure<KestrelServerOptions>(s =>
        {
            s.AddServerHeader = false;

            s.Limits.MaxRequestBufferSize = 128 * 1024;
            s.Limits.MaxRequestLineSize = 32 * 1024;
            s.Limits.MaxRequestHeadersTotalSize = 64 * 1024;
            s.Limits.MaxConcurrentUpgradedConnections = 10000;
        });
    }

    private static IServiceCollection AddRequestTracking(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IRequestTracker>(services =>
        {
            var settings = services.GetFromOptionsOrDefault<DiagnosticFeaturesSettings>();

            if (!settings.AddCurrentRequestsInfoProvider)
                return new DevNullRequestTracker();

            var requestTracker = new RequestTracker();
            AddRequestTrackingInfo(services, requestTracker);

            return requestTracker;
        });
    }

    private static void AddRequestTrackingInfo(IServiceProvider services, RequestTracker requestTracker)
    {
        var diagnostics = services.GetService<IDiagnosticInfo>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, DiagnosticConstants.RequestsInProgressEntry);
        var infoProvider = new CurrentRequestsInfoProvider(requestTracker);

        services.RegisterDisposable(diagnostics.RegisterProvider(infoEntry, infoProvider));
    }

    private static IServiceCollection AddThrottling(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ThrottlingConfigurationBuilder>(services =>
        {
            var builder = new ThrottlingConfigurationBuilder();

            AddThrottlingCpuLimits(services, builder);
            AddThrottlingErrorLogging(services, builder);
            AddThrottlingQuotas(services, builder);

            return builder;
        });
        
        serviceCollection.AddSingleton<IThrottlingProvider>(services =>
        {
            var builder = services.GetRequiredService<ThrottlingConfigurationBuilder>();

            var provider = new ThrottlingProvider(builder.Build());

            AddThrottlingObservability(services, provider);

            return provider;
        });

        return serviceCollection;
    }

    private static void AddThrottlingCpuLimits(IServiceProvider services, ThrottlingConfigurationBuilder builder)
    {
        var limits = services.GetRequiredService<IVostokApplicationLimits>();

        builder.SetNumberOfCores(() =>
        {
            if (limits.CpuUnits is {} cpuUnits)
                return (int)Math.Ceiling(cpuUnits);

            return Environment.ProcessorCount;
        });
    }

    private static void AddThrottlingErrorLogging(IServiceProvider services, ThrottlingConfigurationBuilder builder)
    {
        var log = services.GetRequiredService<ILog>().ForContext<ThrottlingMiddleware>();

        builder.SetErrorCallback(ex => log.Error(ex, "Internal failure in request throttling."));
    }

    private static void AddThrottlingQuotas(IServiceProvider services, ThrottlingConfigurationBuilder builder)
    {
        var configuration = services.GetFromOptionsOrThrow<VostokThrottlingConfiguration>();

        if (configuration.UseThreadPoolOverloadQuota)
        {
            var threadPoolQuota = new ThreadPoolOverloadQuota(new ThreadPoolOverloadQuotaOptions());
            builder.AddCustomQuota(threadPoolQuota);
        }
    }

    private static void AddThrottlingObservability(IServiceProvider services, ThrottlingProvider throttlingProvider)
    {
        var diagnosticSettings = services.GetFromOptionsOrDefault<DiagnosticFeaturesSettings>();

        AddThrottlingMetrics(services, throttlingProvider);

        if (diagnosticSettings.AddThrottlingInfoProvider)
            AddThrottlingInfo(services, throttlingProvider);

        if (diagnosticSettings.AddThrottlingHealthCheck)
            AddThrottlingHealthCheck(services, throttlingProvider);
    }

    private static void AddThrottlingMetrics(IServiceProvider services, ThrottlingProvider throttlingProvider)
    {
        var configuration = services.GetFromOptionsOrDefault<VostokThrottlingConfiguration>();
        if (!configuration.EnableMetrics)
            return;

        var metrics = services.GetRequiredService<IVostokApplicationMetrics>();
        var metricsOptions = services.GetFromOptionsOrDefault<ThrottlingMetricsOptions>();
        
        services.RegisterDisposable(metrics.Instance.CreateThrottlingMetrics(throttlingProvider, metricsOptions));
    }

    private static void AddThrottlingInfo(IServiceProvider services, ThrottlingProvider throttlingProvider)
    {
        var diagnostics = services.GetService<IDiagnosticInfo>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, DiagnosticConstants.RequestThrottlingEntry);
        var infoProvider = new ThrottlingInfoProvider(throttlingProvider);

        services.RegisterDisposable(diagnostics.RegisterProvider(infoEntry, infoProvider));
    }

    private static void AddThrottlingHealthCheck(IServiceProvider services, ThrottlingProvider throttlingProvider)
    {
        var healthTracker = services.GetService<IHealthTracker>();
        if (healthTracker == null)
            return;

        var healthCheck = new ThrottlingHealthCheck(throttlingProvider);

        services.RegisterDisposable(healthTracker.RegisterCheck("Request throttling", healthCheck));
    }
}