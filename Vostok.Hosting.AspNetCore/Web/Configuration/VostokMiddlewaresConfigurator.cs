using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

internal sealed class VostokMiddlewaresConfigurator : IVostokMiddlewaresConfigurator
{
    private readonly IServiceCollection serviceCollection;

    public VostokMiddlewaresConfigurator(IServiceCollection serviceCollection) =>
        this.serviceCollection = serviceCollection;

    public IVostokMiddlewaresConfigurator ConfigureHttpContextTweaks(Action<HttpContextTweakSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureRequestInfoFilling(Action<FillRequestInfoSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureDistributedContext(Action<DistributedContextSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureTracing(Action<TracingSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureThrottling(Action<IVostokThrottlingConfigurator> configure)
    {
        configure(new VostokThrottlingConfigurator(serviceCollection));
        return this;
    }

    public IVostokMiddlewaresConfigurator ConfigureLogging(Action<LoggingSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureDatacenterAwareness(Action<DatacenterAwarenessSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureUnhandledExceptions(Action<UnhandledExceptionSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigurePingApi(Action<PingApiSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureDiagnosticApi(Action<DiagnosticApiSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureDiagnosticFeatures(Action<DiagnosticFeaturesSettings> configure) =>
        Configure(configure);

    public IVostokMiddlewaresConfigurator ConfigureOptions(Action<VostokMiddlewaresConfiguration> configure) =>
        Configure(configure);

    private IVostokMiddlewaresConfigurator Configure<T>(Action<T> configure)
        where T : class
    {
        serviceCollection.Configure(configure);

        return this;
    }
}