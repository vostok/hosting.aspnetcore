using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.AspNetCore.Builders.Throttling;
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

    public static IServiceCollection AddThrottling(this IServiceCollection services)
    {
        services.TryAddSingleton<IThrottlingProvider>(sp =>
        {
            var builder = new ThrottlingConfigurationBuilder();

            AddThrottlingCpuLimits(sp, builder);
            AddThrottlingErrorLogging(sp, builder);
            AddThrottlingQuotas(sp, builder);

            var provider = new ThrottlingProvider(builder.Build());

            AddThrottlingObservability(sp, provider);

            return provider;
        });

        AdaptMiddlewareSettings(services);
        return services;
    }

    private static void AdaptMiddlewareSettings(IServiceCollection services)
    {
        services.AddOptions<ThrottlingSettings>()
            .Configure<IOptions<NewThrottlingSettings>>((middlewareSettings, options) =>
            {
                var throttlingSettings = options.Value;

                middlewareSettings.RejectionResponseCode = throttlingSettings.RejectionResponseCode;
                middlewareSettings.DisableForWebSockets = throttlingSettings.DisableForWebSockets;

                middlewareSettings.AddConsumerProperty = throttlingSettings.Quotas.Any(q => q.Name == WellKnownThrottlingProperties.Consumer);
                middlewareSettings.AddMethodProperty = throttlingSettings.Quotas.Any(q => q.Name == WellKnownThrottlingProperties.Method);
                middlewareSettings.AddPriorityProperty = throttlingSettings.Quotas.Any(q => q.Name == WellKnownThrottlingProperties.Priority);
                middlewareSettings.AddUrlProperty = throttlingSettings.Quotas.Any(q => q.Name == WellKnownThrottlingProperties.Url);

                foreach (var (name, valueExtractor) in throttlingSettings.Properties)
                {
                    middlewareSettings.AdditionalProperties.Add(context => (name, valueExtractor(context)));
                }
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

    private static void AddThrottlingQuotas(IServiceProvider serviceProvider, ThrottlingConfigurationBuilder builder)
    {
        var settings = serviceProvider.GetFromOptionsOrDefault<NewThrottlingSettings>();

        if (settings.UseThreadPoolOverloadQuota)
        {
            var threadPoolQuota = new ThreadPoolOverloadQuota(new ThreadPoolOverloadQuotaOptions());
            builder.AddCustomQuota(threadPoolQuota);
        }

        foreach (var (propertyName, quotaOptionsProvider) in settings.Quotas)
        {
            builder.SetPropertyQuota(propertyName, quotaOptionsProvider);
        }

        builder.SetEssentials(settings.Essentials);
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
        var settings = serviceProvider.GetFromOptionsOrDefault<NewThrottlingSettings>();

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

    private static void AddRequestTrackingInfo(IServiceProvider serviceProvider, RequestTracker requestTracker)
    {
        var diagnostics = serviceProvider.GetService<IDiagnosticInfo>();
        if (diagnostics == null)
            return;

        var infoEntry = new DiagnosticEntry(DiagnosticConstants.Component, DiagnosticConstants.RequestsInProgressEntry);
        var infoProvider = new CurrentRequestsInfoProvider(requestTracker);

        serviceProvider.RegisterDisposable(diagnostics.RegisterProvider(infoEntry, infoProvider));
    }
}