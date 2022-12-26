using System;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Builders.Throttling;

public interface IHostingThrottlingBuilder
{
    IHostingThrottlingBuilder UseEssentials(Func<ThrottlingEssentials> essentialsProvider);
    IHostingThrottlingBuilder UsePropertyQuota(string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider);
    IHostingThrottlingBuilder UseCustomQuota(IThrottlingQuota quota);
    IHostingThrottlingBuilder UseThreadPoolOverloadQuota(bool value);
    IHostingThrottlingBuilder ConfigureMiddleware(Action<ThrottlingSettings> configure);

    IHostingThrottlingBuilder DisableMetrics();
    IHostingThrottlingBuilder ConfigureMetrics(Action<ThrottlingMetricsOptions> metrics);
}