using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Builders.Throttling;

namespace Vostok.Hosting.AspNetCore.Builders.Middlewares;

internal sealed class VostokMiddlewaresBuilder : IVostokMiddlewaresBuilder
{
    private readonly IServiceCollection services;

    public VostokMiddlewaresBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public IVostokMiddlewaresBuilder ConfigureHttpContextTweaks(Action<HttpContextTweakSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureRequestInfoFilling(Action<FillRequestInfoSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureDistributedContext(Action<DistributedContextSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureTracing(Action<TracingSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureThrottling(Action<IHostingThrottlingBuilder> configure)
    {
        configure(new HostingThrottlingBuilder(services));
        return this;
    }

    public IVostokMiddlewaresBuilder ConfigureRequestLogging(Action<LoggingSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureDatacenterAwareness(Action<DatacenterAwarenessSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureUnhandledExceptions(Action<UnhandledExceptionSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigurePingApi(Action<PingApiSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureDiagnosticApi(Action<DiagnosticApiSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureDiagnosticFeatures(Action<DiagnosticFeaturesSettings> configure) =>
        Configure(configure);

    private IVostokMiddlewaresBuilder Configure<T>(Action<T> configure)
        where T : class
    {
        services.Configure(configure);

        return this;
    }
}