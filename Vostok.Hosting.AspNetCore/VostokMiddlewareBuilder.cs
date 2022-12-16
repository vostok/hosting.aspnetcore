using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public sealed class VostokMiddlewareBuilder
{
    internal Action<HttpContextTweakSettings> HttpContextTweaks = _ => {};
    internal Action<FillRequestInfoSettings> FillRequestInfo = _ => {};
    internal Action<DistributedContextSettings> DistributedContext = _ => {};
    internal Action<TracingSettings> Tracing = _ => {};
    internal Action<ThrottlingSettings> ThrottlingMiddleware = _ => {};
    internal Action<VostokThrottlingSettings> Throttling = _ => {};
    internal Action<LoggingSettings> RequestLogging = _ => {};
    internal Action<DatacenterAwarenessSettings> DatacenterAwareness = _ => {};
    internal Action<UnhandledExceptionSettings> UnhandledExceptions = _ => {};
    internal Action<PingApiSettings> PingApi = _ => {};
    internal Action<DiagnosticApiSettings> DiagnosticApi = _ => {};
    internal Action<DiagnosticFeaturesSettings> DiagnosticFeatures = _ => {};

    public VostokMiddlewareBuilder ConfigureHttpContextTweaks(Action<HttpContextTweakSettings> configure)
    {
        HttpContextTweaks = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureFillRequestInfo(Action<FillRequestInfoSettings> configure)
    {
        FillRequestInfo = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureDistributedContext(Action<DistributedContextSettings> configure)
    {
        DistributedContext = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureTracing(Action<TracingSettings> configure)
    {
        Tracing = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureThrottling(Action<ThrottlingSettings> configureMiddleware, Action<VostokThrottlingSettings> configureQuotas)
    {
        ThrottlingMiddleware = configureMiddleware;
        Throttling = configureQuotas;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureRequestLogging(Action<LoggingSettings> configure)
    {
        RequestLogging = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureDatacenterAwareness(Action<DatacenterAwarenessSettings> configure)
    {
        DatacenterAwareness = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureDatacenterAwareness(Action<UnhandledExceptionSettings> configure)
    {
        UnhandledExceptions = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigurePingApi(Action<PingApiSettings> configure)
    {
        PingApi = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureDiagnosticApi(Action<DiagnosticApiSettings> configure)
    {
        DiagnosticApi = configure;
        return this;
    }

    public VostokMiddlewareBuilder ConfigureDiagnosticFeatures(Action<DiagnosticFeaturesSettings> configure)
    {
        DiagnosticFeatures = configure;
        return this;
    }
}