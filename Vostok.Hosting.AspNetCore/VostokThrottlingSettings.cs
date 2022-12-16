using System;
using JetBrains.Annotations;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public class VostokThrottlingSettings
{
    private readonly ThrottlingConfigurationBuilder configurationBuilder = new();

    public bool UseThreadPoolOverloadQuota { get; set; } = true;
    public ThrottlingMetricsOptions? Metrics { get; set; } = new();

    public VostokThrottlingSettings UseEssentials(Func<ThrottlingEssentials> essentialsProvider)
    {
        configurationBuilder.SetEssentials(essentialsProvider);
        return this;
    }

    public VostokThrottlingSettings UsePropertyQuota(string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        configurationBuilder.SetPropertyQuota(propertyName, quotaOptionsProvider);
        return this;
    }

    public VostokThrottlingSettings UseCustomQuota(IThrottlingQuota quota)
    {
        configurationBuilder.AddCustomQuota(quota);
        return this;
    }

    internal ThrottlingProvider BuildProvider()
    {
        if (UseThreadPoolOverloadQuota)
        {
            var threadPoolQuota = new ThreadPoolOverloadQuota(new ThreadPoolOverloadQuotaOptions());
            configurationBuilder.AddCustomQuota(threadPoolQuota);
        }

        return new ThrottlingProvider(configurationBuilder.Build());
    }
}