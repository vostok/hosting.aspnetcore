using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Applications.AspNetCore;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;
using Vostok.Throttling;
using Vostok.Throttling.Metrics;

namespace Vostok.Hosting.AspNetCore.Extensions;

[PublicAPI]
public static class IServiceCollectionExtensions
{
    public static void ConfigureShutdownTimeout(this IServiceCollection serviceCollection, TimeSpan timeout) =>
        serviceCollection.Configure<HostOptions>(
            opts => opts.ShutdownTimeout = timeout);

    public static IServiceCollection AddVostokMiddlewares(this IServiceCollection services, Action<VostokMiddlewareBuilder> build)
    {
        var builder = new VostokMiddlewareBuilder();
        build(builder);

        services
            .Configure(builder.DiagnosticFeatures)
            .Configure(builder.Throttling)
            .AddThrottlingProvider()
            .AddRequestTracking();

        return services
            .AddVostokHttpContextTweaks(builder.HttpContextTweaks)
            .AddVostokRequestInfo(builder.FillRequestInfo)
            .AddVostokDistributedContext(builder.DistributedContext)
            .AddVostokTracing(builder.Tracing)
            // .AddVostokThrottling(null, builder.ThrottlingMiddleware) // todo: overload without IThrottlingProvider
            .Configure(builder.ThrottlingMiddleware)
            .AddVostokRequestLogging(builder.RequestLogging)
            .AddVostokDatacenterAwareness(builder.DatacenterAwareness)
            .AddVostokUnhandledExceptions(builder.UnhandledExceptions)
            .AddVostokPingApi(builder.PingApi)
            .AddVostokDiagnosticApi(builder.DiagnosticApi);
    }

    internal static void AddVostokLoggerProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.RemoveAll<ILoggerProvider>();

        serviceCollection.Configure<VostokLoggerProviderSettings>(settings =>
            settings.IgnoredScopePrefixes = new[] {"Microsoft.AspNetCore"});

        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, VostokLoggerProvider>(serviceProvider =>
            new VostokLoggerProvider(serviceProvider.GetRequiredService<ILog>(), serviceProvider.GetFromOptionsOrDefault<VostokLoggerProviderSettings>())));
    }

    private static IServiceCollection AddThrottlingProvider(this IServiceCollection services)
    {
        return services.AddSingleton<IThrottlingProvider>(sp =>
        {
            var throttlingProvider = sp.GetFromOptionsOrDefault<VostokThrottlingSettings>().BuildProvider();
            var diagnosticSettings = sp.GetFromOptionsOrDefault<DiagnosticFeaturesSettings>();

            AddThrottlingMetrics(sp, throttlingProvider);

            if (diagnosticSettings.AddThrottlingInfoProvider)
                AddThrottlingInfo(sp, throttlingProvider);

            if (diagnosticSettings.AddThrottlingHealthCheck)
                AddThrottlingHealthCheck(sp, throttlingProvider);

            return throttlingProvider;
        });
    }

    private static void AddThrottlingMetrics(IServiceProvider serviceProvider, ThrottlingProvider throttlingProvider)
    {
        var metrics = serviceProvider.GetService<IVostokApplicationMetrics>();
        var settings = serviceProvider.GetFromOptionsOrDefault<VostokThrottlingSettings>();

        if (settings.Metrics == null || metrics == null)
            return;

        serviceProvider.RegisterDisposable(metrics.Instance.CreateThrottlingMetrics(throttlingProvider, settings.Metrics));
    }

    private static void AddThrottlingInfo(IServiceProvider serviceProvider, ThrottlingProvider throttlingProvider)
    {
        var diagnostics = serviceProvider.GetService<IVostokApplicationDiagnostics>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, "request-throttling");
        var infoProvider = new ThrottlingInfoProvider(throttlingProvider);

        serviceProvider.RegisterDisposable(diagnostics.Info.RegisterProvider(infoEntry, infoProvider));
    }

    private static void AddThrottlingHealthCheck(IServiceProvider serviceProvider, ThrottlingProvider throttlingProvider)
    {
        var diagnostics = serviceProvider.GetService<IVostokApplicationDiagnostics>();
        if (diagnostics == null)
            return;

        var healthCheck = new ThrottlingHealthCheck(throttlingProvider);

        serviceProvider.RegisterDisposable(diagnostics.HealthTracker.RegisterCheck("Request throttling", healthCheck));
    }

    private static IServiceCollection AddRequestTracking(this IServiceCollection services)
    {
        return services.AddSingleton<IRequestTracker>(sp =>
        {
            var settings = sp.GetFromOptionsOrDefault<DiagnosticFeaturesSettings>();

            if (!settings.AddCurrentRequestsInfoProvider)
                return new DevNullRequestTracker();

            var requestTracker = new RequestTracker();
            AddRequestTrackingInfo(sp, requestTracker);

            return requestTracker;
        });
    }

    private static void AddRequestTrackingInfo(IServiceProvider serviceProvider, RequestTracker requestTracker)
    {
        var diagnostics = serviceProvider.GetService<IVostokApplicationDiagnostics>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, "requests-in-progress");
        var infoProvider = new CurrentRequestsInfoProvider(requestTracker);

        serviceProvider.RegisterDisposable(diagnostics.Info.RegisterProvider(infoEntry, infoProvider));
    }
}