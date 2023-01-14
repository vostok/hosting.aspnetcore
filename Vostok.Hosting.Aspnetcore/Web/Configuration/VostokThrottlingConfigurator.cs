using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

internal sealed class VostokThrottlingConfigurator : IVostokThrottlingConfigurator
{
    private readonly IServiceCollection services;

    public VostokThrottlingConfigurator(IServiceCollection services) =>
        this.services = services;

    public bool UseThreadPoolOverloadQuota
    {
        set => Configure(config => config.UseThreadPoolOverloadQuota = value);
    }
    
    public bool EnableMetrics
    {
        set => Configure(config => config.EnableMetrics = value);
    }

    public IVostokThrottlingConfigurator ConfigureMetrics(Action<ThrottlingMetricsOptions> configure) =>
        Configure(configure);

    public IVostokThrottlingConfigurator UseEssentials(Func<ThrottlingEssentials> essentialsProvider) =>
        ConfigureBuilder(builder => builder.SetEssentials(essentialsProvider));

    public IVostokThrottlingConfigurator UsePropertyQuota(string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        ConfigureBuilder(builder => builder.SetPropertyQuota(propertyName, quotaOptionsProvider));

    public IVostokThrottlingConfigurator UseCustomQuota(IThrottlingQuota quota) =>
        ConfigureBuilder(builder => builder.AddCustomQuota(quota));

    public IVostokThrottlingConfigurator ConfigureMiddleware(Action<ThrottlingSettings> configure) =>
        Configure(configure);
    
    private IVostokThrottlingConfigurator ConfigureBuilder(Action<ThrottlingConfigurationBuilder> configure) =>
        Configure(configure);
    
    private IVostokThrottlingConfigurator Configure(Action<VostokThrottlingConfiguration> configure) =>
        Configure<VostokThrottlingConfiguration>(configure);
    
    private IVostokThrottlingConfigurator Configure<T>(Action<T> configure)
        where T : class
    {
        services.Configure(configure);

        return this;
    }
}