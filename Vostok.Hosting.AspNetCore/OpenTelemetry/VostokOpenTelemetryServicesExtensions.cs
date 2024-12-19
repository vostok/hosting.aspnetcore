using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Vostok.Hosting.AspNetCore.OpenTelemetry.ResourceDetectors;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

internal static class VostokOpenTelemetryServicesExtensions
{
    public static void ConfigureOpenTelemetryForVostok(this IServiceCollection services) =>
        services.ConfigureOpenTelemetryTracerProvider(ConfigureForVostok)
                .ConfigureOpenTelemetryMeterProvider(ConfigureForVostok)
                .ConfigureOpenTelemetryLoggerProvider(ConfigureForVostok);

    private static void ConfigureForVostok(TracerProviderBuilder tracing) =>
        tracing.AddSource("Vostok.Tracer")
               .AddAspNetCoreInstrumentation()
               .ConfigureServices(services => services.ConfigureAspNetCoreInstrumentationForVostok())
               .ConfigureResource(ConfigureVostokTracingResource);

    private static void ConfigureForVostok(MeterProviderBuilder metrics) =>
        metrics.ConfigureResource(ConfigureVostokMetricsResource);

    public static void ConfigureForVostok(LoggerProviderBuilder logging) =>
        logging.ConfigureResource(ConfigureVostokLoggingResource);

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