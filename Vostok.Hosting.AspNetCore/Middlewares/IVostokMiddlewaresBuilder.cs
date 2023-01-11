using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Middlewares.Configuration;
using Vostok.Hosting.AspNetCore.Middlewares.Diagnostics;
using ThrottlingSettings = Vostok.Hosting.AspNetCore.Middlewares.Configuration.ThrottlingSettings;

namespace Vostok.Hosting.AspNetCore.Middlewares;

[PublicAPI]
public interface IVostokMiddlewaresBuilder
{
    public IVostokMiddlewaresBuilder ConfigureHttpContextTweaks(Action<HttpContextTweakSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureRequestInfoFilling(Action<FillRequestInfoSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureDistributedContext(Action<DistributedContextSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureTracing(Action<TracingSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureThrottling(Action<ThrottlingSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureRequestLogging(Action<LoggingSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureDatacenterAwareness(Action<DatacenterAwarenessSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureUnhandledExceptions(Action<UnhandledExceptionSettings> configure);
    public IVostokMiddlewaresBuilder ConfigurePingApi(Action<PingApiSettings> configure);
    public IVostokMiddlewaresBuilder ConfigureDiagnostics(Action<IHostingDiagnosticsBuilder> configure);
    public IVostokMiddlewaresBuilder ConfigureEnabled(Action<VostokMiddlewaresEnabledSettings> configure);
}