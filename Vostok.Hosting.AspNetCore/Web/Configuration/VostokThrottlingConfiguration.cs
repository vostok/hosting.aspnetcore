namespace Vostok.Hosting.AspNetCore.Web.Configuration;

internal class VostokThrottlingConfiguration
{
    public bool UseThreadPoolOverloadQuota { get; set; } = true;

    public bool EnableMetrics { get; set; } = true;
}