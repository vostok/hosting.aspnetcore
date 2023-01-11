using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Throttling;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Builders.Throttling;

[PublicAPI]
public class NewThrottlingSettings
{
    public ThrottlingMetricsOptions? Metrics { get; set; } = new();
    public bool UseThreadPoolOverloadQuota { get; set; }

    public int RejectionResponseCode { get; set; } = 429;
    public bool DisableForWebSockets { get; set; } = true;

    public List<ThrottlingQuota> Quotas { get; set; } = new();
    public List<ThrottlingProperty> Properties { get; set; } = new();

    public void UseConsumerQuota(Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        Quotas.Add(new ThrottlingQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider));

    public void UsePriorityQuota(Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        Quotas.Add(new ThrottlingQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider));

    public void UseMethodQuota(Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        Quotas.Add(new ThrottlingQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider));

    public void UseUrlQuota(Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        Quotas.Add(new ThrottlingQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider));

    public void UseCustomPropertyQuota(string propertyName, Func<HttpContext, string> propertyValueProvider, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        Properties.Add(new ThrottlingProperty(propertyName, propertyValueProvider));
        Quotas.Add(new ThrottlingQuota(propertyName, quotaOptionsProvider));
    }
}

public sealed record ThrottlingQuota(string Name, Func<PropertyQuotaOptions> QuotaOptionsProvider);

public sealed record ThrottlingProperty(string Name, Func<HttpContext, string> ValueExtractor);