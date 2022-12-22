using System;
using JetBrains.Annotations;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public class VostokThrottlingSettings
{
    internal readonly ThrottlingConfigurationBuilder ConfigurationBuilder = new();

    public bool UseThreadPoolOverloadQuota { get; set; } = true;
    public ThrottlingMetricsOptions? Metrics { get; set; } = new();

    public VostokThrottlingSettings UseEssentials(Func<ThrottlingEssentials> essentialsProvider)
    {
        ConfigurationBuilder.SetEssentials(essentialsProvider);
        return this;
    }

    public VostokThrottlingSettings UsePropertyQuota(string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        ConfigurationBuilder.SetPropertyQuota(propertyName, quotaOptionsProvider);
        return this;
    }

    public VostokThrottlingSettings UseCustomQuota(IThrottlingQuota quota)
    {
        ConfigurationBuilder.AddCustomQuota(quota);
        return this;
    }
}