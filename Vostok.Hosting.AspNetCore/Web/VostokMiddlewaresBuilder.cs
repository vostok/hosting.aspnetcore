using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Web.Configuration;
using Vostok.Hosting.AspNetCore.Web.Diagnostics;
using ThrottlingSettings = Vostok.Hosting.AspNetCore.Web.Configuration.ThrottlingSettings;

namespace Vostok.Hosting.AspNetCore.Web;

internal sealed class VostokMiddlewaresBuilder : IVostokMiddlewaresBuilder
{
    private readonly IServiceCollection services;

    public VostokMiddlewaresBuilder(IServiceCollection services) =>
        this.services = services;

    public IVostokMiddlewaresBuilder ConfigureHttpContextTweaks(Action<HttpContextTweakSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureRequestInfoFilling(Action<FillRequestInfoSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureDistributedContext(Action<DistributedContextSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureTracing(Action<TracingSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureThrottling(Action<ThrottlingSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureRequestLogging(Action<LoggingSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureDatacenterAwareness(Action<DatacenterAwarenessSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureUnhandledExceptions(Action<UnhandledExceptionSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigurePingApi(Action<PingApiSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresBuilder ConfigureDiagnostics(Action<IHostingDiagnosticsBuilder> configure)
    {
        configure(new HostingDiagnosticsBuilder(services));
        return this;
    }

    public IVostokMiddlewaresBuilder ConfigureEnabled(Action<VostokMiddlewaresEnabledSettings> configure) =>
        Configure(configure);

    private IVostokMiddlewaresBuilder Configure<T>(Action<T> configure)
        where T : class
    {
        services.Configure(configure);

        return this;
    }
}