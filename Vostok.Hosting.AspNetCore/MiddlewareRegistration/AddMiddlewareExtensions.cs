using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Logging.Abstractions;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.MiddlewareRegistration;

internal static class AddMiddlewareExtensions
{
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

    public static IServiceCollection AddThrottling(this IServiceCollection services, Action<ThrottlingSettings> configure)
    {
        services.Configure(configure);

        return services.AddSingleton<IThrottlingProvider>(sp =>
        {
            var builder = sp.GetFromOptionsOrDefault<VostokThrottlingSettings>().ConfigurationBuilder;

            AddThrottlingCpuLimits(sp, builder);
            AddThrottlingErrorLogging(sp, builder);
            AddThreadPoolOverloadQuota(sp, builder);

            var provider = new ThrottlingProvider(builder.Build());

            AddThrottlingObservability(sp, provider);

            return provider;
        });
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

    private static void AddThreadPoolOverloadQuota(IServiceProvider serviceProvider, ThrottlingConfigurationBuilder builder)
    {
        var settings = serviceProvider.GetFromOptionsOrDefault<VostokThrottlingSettings>();

        if (settings.UseThreadPoolOverloadQuota)
        {
            var threadPoolQuota = new ThreadPoolOverloadQuota(new ThreadPoolOverloadQuotaOptions());
            builder.AddCustomQuota(threadPoolQuota);
        }
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