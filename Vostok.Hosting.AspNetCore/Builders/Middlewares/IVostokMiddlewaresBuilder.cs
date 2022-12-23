using System;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Builders.Throttling;

namespace Vostok.Hosting.AspNetCore.Builders.Middlewares;

public interface IVostokMiddlewaresBuilder
{
    public IVostokMiddlewaresBuilder ConfigureHttpContextTweaks(Action<HttpContextTweakSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureRequestInfoFilling(Action<FillRequestInfoSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureDistributedContext(Action<DistributedContextSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureTracing(Action<TracingSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureThrottling(Action<IHostingThrottlingBuilder> configure);
    public IVostokMiddlewaresBuilder ConfigureRequestLogging(Action<LoggingSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureDatacenterAwareness(Action<DatacenterAwarenessSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureUnhandledExceptions(Action<UnhandledExceptionSettings> configure);
    public IVostokMiddlewaresBuilder ConfigurePingApi(Action<PingApiSettings> configure);

    // TODO: Use single builder for diagnostics
    public IVostokMiddlewaresBuilder ConfigureDiagnosticApi(Action<DiagnosticApiSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureDiagnosticFeatures(Action<DiagnosticFeaturesSettings> configure);
}