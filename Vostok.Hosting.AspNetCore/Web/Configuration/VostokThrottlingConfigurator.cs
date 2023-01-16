using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

internal sealed class VostokThrottlingConfigurator : IVostokThrottlingConfigurator
{
    private readonly IServiceCollection serviceCollection;

    public VostokThrottlingConfigurator(IServiceCollection serviceCollection) =>
        this.serviceCollection = serviceCollection;

    public IVostokThrottlingConfigurator ConfigureMetrics(Action<ThrottlingMetricsOptions> configure) =>
        Configure(configure);
    
    public IVostokThrottlingConfigurator ConfigureMiddleware(Action<ThrottlingSettings> configure) =>
        Configure(configure);
    
    public IVostokThrottlingConfigurator ConfigureBuilder(Action<ThrottlingConfigurationBuilder> configure) =>
        Configure(configure);
    
    public IVostokThrottlingConfigurator ConfigureOptions(Action<VostokThrottlingConfiguration> configure) =>
        Configure(configure);
    
    private IVostokThrottlingConfigurator Configure<T>(Action<T> configure)
        where T : class
    {
        serviceCollection.Configure(configure);
        return this;
    }
}