using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
using ThrottlingSettings = Vostok.Hosting.AspNetCore.Web.Configuration.ThrottlingSettings;

namespace Vostok.Hosting.AspNetCore.Web;

/// <summary>
/// <para>Register Vostok middlewares and their configuration in DI container.</para>
/// <para>Use returning <see cref="IVostokMiddlewaresBuilder"/> to configure them.</para>
/// <para>Applies reasonable Kestrel defaults.</para>
/// </summary>
public static class AddVostokMiddlewaresExtensions
{
    /// <inheritdoc cref="AddVostokMiddlewaresExtensions"/>
    public static IVostokMiddlewaresBuilder AddVostokMiddlewares(this IServiceCollection serviceCollection)
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

        return new VostokMiddlewaresBuilder(serviceCollection);
    }

    private static IServiceCollection ConfigureKestrelDefaults(this IServiceCollection services)
    {
        return services.Configure<KestrelServerOptions>(s =>
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

    private static void AddRequestTrackingInfo(IServiceProvider serviceProvider, RequestTracker requestTracker)
    {
        var diagnostics = serviceProvider.GetService<IDiagnosticInfo>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, DiagnosticConstants.RequestsInProgressEntry);
        var infoProvider = new CurrentRequestsInfoProvider(requestTracker);

        serviceProvider.RegisterDisposable(diagnostics.RegisterProvider(infoEntry, infoProvider));
    }

    private static IServiceCollection AddThrottling(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IThrottlingProvider>(services =>
        {
            var builder = new ThrottlingConfigurationBuilder();

            AddThrottlingCpuLimits(services, builder);
            AddThrottlingErrorLogging(services, builder);
            AddThrottlingQuotas(services, builder);

            var provider = new ThrottlingProvider(builder.Build());

            AddThrottlingObservability(services, provider);

            return provider;
        });

        AdaptMiddlewareSettings(serviceCollection);

        return serviceCollection;
    }

    private static void AddThrottlingCpuLimits(IServiceProvider serviceProvider, ThrottlingConfigurationBuilder builder)
    {
        var limits = serviceProvider.GetRequiredService<IVostokApplicationLimits>();

        builder.SetNumberOfCores(() =>
        {
            if (limits.CpuUnits is {} cpuUnits)
                return (int)Math.Ceiling(cpuUnits);

            return Environment.ProcessorCount;
        });
    }

    private static void AddThrottlingErrorLogging(IServiceProvider serviceProvider, ThrottlingConfigurationBuilder builder)
    {
        var log = serviceProvider.GetRequiredService<ILog>().ForContext<ThrottlingMiddleware>();

        builder.SetErrorCallback(ex => log.Error(ex, "Internal failure in request throttling."));
    }

    private static void AddThrottlingQuotas(IServiceProvider serviceProvider, ThrottlingConfigurationBuilder builder)
    {
        var settings = serviceProvider.GetFromOptionsOrDefault<ThrottlingSettings>();

        if (settings.UseThreadPoolOverloadQuota)
        {
            var threadPoolQuota = new ThreadPoolOverloadQuota(new ThreadPoolOverloadQuotaOptions());
            builder.AddCustomQuota(threadPoolQuota);
        }

        foreach (var (propertyName, quotaOptionsProvider) in settings.Quotas)
            builder.SetPropertyQuota(propertyName, quotaOptionsProvider);
    }

    private static void AddThrottlingObservability(IServiceProvider serviceProvider, ThrottlingProvider throttlingProvider)
    {
        var diagnosticSettings = serviceProvider.GetFromOptionsOrDefault<DiagnosticFeaturesSettings>();

        AddThrottlingMetrics(serviceProvider, throttlingProvider);

        if (diagnosticSettings.AddThrottlingInfoProvider)
            AddThrottlingInfo(serviceProvider, throttlingProvider);

        if (diagnosticSettings.AddThrottlingHealthCheck)
            AddThrottlingHealthCheck(serviceProvider, throttlingProvider);
    }

    private static void AddThrottlingMetrics(IServiceProvider serviceProvider, ThrottlingProvider throttlingProvider)
    {
        var metrics = serviceProvider.GetService<IVostokApplicationMetrics>();
        var settings = serviceProvider.GetFromOptionsOrDefault<ThrottlingSettings>();

        if (settings.Metrics == null || metrics == null)
            return;

        serviceProvider.RegisterDisposable(metrics.Instance.CreateThrottlingMetrics(throttlingProvider, settings.Metrics));
    }

    private static void AddThrottlingInfo(IServiceProvider serviceProvider, ThrottlingProvider throttlingProvider)
    {
        var diagnostics = serviceProvider.GetService<IDiagnosticInfo>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, DiagnosticConstants.RequestThrottlingEntry);
        var infoProvider = new ThrottlingInfoProvider(throttlingProvider);

        serviceProvider.RegisterDisposable(diagnostics.RegisterProvider(infoEntry, infoProvider));
    }

    private static void AddThrottlingHealthCheck(IServiceProvider serviceProvider, ThrottlingProvider throttlingProvider)
    {
        var healthTracker = serviceProvider.GetService<IHealthTracker>();
        if (healthTracker == null)
            return;

        var healthCheck = new ThrottlingHealthCheck(throttlingProvider);

        serviceProvider.RegisterDisposable(healthTracker.RegisterCheck("Request throttling", healthCheck));
    }

    private static void AdaptMiddlewareSettings(IServiceCollection services)
    {
        services.AddOptions<Applications.AspNetCore.Configuration.ThrottlingSettings>()
            .Configure<IOptions<ThrottlingSettings>>((middlewareSettings, options) =>
            {
                var throttlingSettings = options.Value;

                bool HasQuota(string name) =>
                    throttlingSettings.Quotas.Any(q => q.PropertyName == name);

                middlewareSettings.RejectionResponseCode = throttlingSettings.RejectionResponseCode;
                middlewareSettings.DisableForWebSockets = throttlingSettings.DisableForWebSockets;

                middlewareSettings.AddConsumerProperty = HasQuota(WellKnownThrottlingProperties.Consumer);
                middlewareSettings.AddMethodProperty = HasQuota(WellKnownThrottlingProperties.Method);
                middlewareSettings.AddPriorityProperty = HasQuota(WellKnownThrottlingProperties.Priority);
                middlewareSettings.AddUrlProperty = HasQuota(WellKnownThrottlingProperties.Url);

                foreach (var (name, valueExtractor) in throttlingSettings.Properties)
                    middlewareSettings.AdditionalProperties.Add(context => (name, valueExtractor(context)));
            });
    }
}