using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;

namespace Vostok.Hosting.AspNetCore.Builders;

public class HostingThrottlingSettings
{
    public ThrottlingConfigurationBuilder ConfigurationBuilder { get; } = new();
    public bool UseThreadPoolOverloadQuota { get; set; } = true;
    public ThrottlingMetricsOptions? Metrics { get; set; } = new();
}