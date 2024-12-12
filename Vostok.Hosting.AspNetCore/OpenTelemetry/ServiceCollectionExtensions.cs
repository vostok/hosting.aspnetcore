using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Vostok.Hosting.AspNetCore.OpenTelemetry.ResourceDetectors;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureOpenTelemetryTracerProviderForVostok(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetryTracerProvider(tracing =>
        {
            tracing.ConfigureResource(ConfigureVostokTracingResource);
        });

        return services;
    }

    public static IServiceCollection ConfigureOpenTelemetryMeterProviderForVostok(
        this IServiceCollection services,
        Action<VostokOpenTelemetryMeterResourceOptions>? configureResourceOptions = null)
    {
        if (configureResourceOptions != null)
            services.Configure(configureResourceOptions);

        services.ConfigureOpenTelemetryMeterProvider(metrics =>
        {
            metrics.ConfigureResource(ConfigureVostokMetricsResource);
        });

        return services;
    }

    public static IServiceCollection ConfigureOpenTelemetryLoggingProviderForVostok(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetryLoggerProvider(logging =>
        {
            logging.ConfigureResource(ConfigureVostokLoggingResource);
        });

        return services;
    }

    private static void ConfigureVostokTracingResource(ResourceBuilder resourceBuilder) =>
        resourceBuilder.Clear()
                       .AddTelemetrySdk()
                       .AddDetector(provider => new ServiceResourceDetector(provider));

    private static void ConfigureVostokMetricsResource(ResourceBuilder resourceBuilder)
    {
        resourceBuilder.Clear()
                       .AddDetector(provider =>
                       {
                           var options = provider.GetRequiredService<IOptions<VostokOpenTelemetryMeterResourceOptions>>();
                           return new VostokIdentityResourceDetector(provider, ToDetectorOptions(options.Value));
                       });
        return;

        static VostokIdentityResourceDetector.AttributesOptions ToDetectorOptions(VostokOpenTelemetryMeterResourceOptions options) =>
            new()
            {
                AddProject = options.AddProject,
                AddSubproject = options.AddSubproject,
                AddEnvironment = options.AddEnvironment,
                AddApplication = options.AddApplication,
                AddInstance = options.AddInstance
            };
    }

    private static void ConfigureVostokLoggingResource(ResourceBuilder resourceBuilder) =>
        resourceBuilder.Clear()
                       .AddTelemetrySdk()
                       .AddDetector(provider => new ServiceResourceDetector(provider))
                       .AddDetector(provider => new VostokIdentityResourceDetector(provider));
}