using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Throttling;
using Vostok.Throttling.Metrics;

namespace Vostok.Hosting.AspNetCore.MiddlewareRegistration;

internal static class AddMiddlewareExtensions
{
    public static IServiceCollection AddThrottling(this IServiceCollection services, Action<ThrottlingSettings> configure)
    {
        services.Configure(configure);

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

    public static IServiceCollection AddRequestTracking(this IServiceCollection services)
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
        var diagnostics = serviceProvider.GetService<IDiagnosticInfo>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, "request-throttling");
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

    private static void AddRequestTrackingInfo(IServiceProvider serviceProvider, RequestTracker requestTracker)
    {
        var diagnostics = serviceProvider.GetService<IDiagnosticInfo>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, "requests-in-progress");
        var infoProvider = new CurrentRequestsInfoProvider(requestTracker);

        serviceProvider.RegisterDisposable(diagnostics.RegisterProvider(infoEntry, infoProvider));
    }
}