using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016")]
public static class OpenTelemetryVostokExtensions
{
    public static OpenTelemetryBuilder ConfigureForVostok(this OpenTelemetryBuilder builder)
    {
        builder.WithTracing(tracing => tracing.ConfigureForVostok())
               .WithMetrics();

        builder.Services.ConfigureOpenTelemetryTracerProviderForVostok();
        builder.Services.ConfigureOpenTelemetryMeterProviderForVostok();
        builder.Services.ConfigureOpenTelemetryLoggingProviderForVostok();

        return builder;
    }

    public static TracerProviderBuilder ConfigureForVostok(this TracerProviderBuilder tracing)
    {
        tracing.AddSource("Vostok.Tracer")
               .AddSource("Vostok.ClusterClient");

        return tracing;
    }
}