using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Builders;

internal sealed class HostingThrottlingBuilder : IHostingThrottlingBuilder
{
    private readonly IServiceCollection services;

    public HostingThrottlingBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public IHostingThrottlingBuilder UseEssentials(Func<ThrottlingEssentials> essentialsProvider)
    {
        services.Configure<HostingThrottlingSettings>(s => s.ConfigurationBuilder.SetEssentials(essentialsProvider));
        return this;
    }

    public IHostingThrottlingBuilder UsePropertyQuota(string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        services.Configure<HostingThrottlingSettings>(s => s.ConfigurationBuilder.SetPropertyQuota(propertyName, quotaOptionsProvider));
        return this;
    }

    public IHostingThrottlingBuilder UseCustomQuota(IThrottlingQuota quota)
    {
        services.Configure<HostingThrottlingSettings>(s => s.ConfigurationBuilder.AddCustomQuota(quota));
        return this;
    }

    public IHostingThrottlingBuilder ConfigureMiddleware(Action<ThrottlingSettings> configure)
    {
        services.Configure(configure);
        return this;
    }

    public IHostingThrottlingBuilder UseThreadPoolOverloadQuota(bool value)
    {
        services.Configure<HostingThrottlingSettings>(s => s.UseThreadPoolOverloadQuota = value);
        return this;
    }

    public IHostingThrottlingBuilder DisableMetrics()
    {
        services.Configure<HostingThrottlingSettings>(s => s.Metrics = null);
        return this;
    }

    public IHostingThrottlingBuilder ConfigureMetrics(Action<ThrottlingMetricsOptions> metrics)
    {
        services.Configure<HostingThrottlingSettings>(s => metrics(s.Metrics ??= new ThrottlingMetricsOptions()));
        return this;
    }
}