using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Middlewares.Configuration;

[PublicAPI]
public static class ThrottlingSettingsExtensions
{
    public static ThrottlingSettings UseQuota(this ThrottlingSettings settings, string quotaName, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        settings.Quotas.Add(new ThrottlingQuota(quotaName, quotaOptionsProvider));
        return settings;
    }

    public static ThrottlingSettings UseConsumerQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider);

    public static ThrottlingSettings UsePriorityQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider);

    public static ThrottlingSettings UseMethodQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider);

    public static ThrottlingSettings UseUrlQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider);

    public static ThrottlingSettings UseEssentials(this ThrottlingSettings settings, Func<ThrottlingEssentials> essentialsProvider)
    {
        settings.Essentials = essentialsProvider;
        return settings;
    }

    public static ThrottlingSettings UseEssentials(this ThrottlingSettings settings, ThrottlingEssentials essentials) =>
        settings.UseEssentials(() => essentials);

    public static ThrottlingSettings UseCustomPropertyQuota(this ThrottlingSettings settings, string propertyName, Func<HttpContext, string> propertyValueProvider, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        settings.Properties.Add(new ThrottlingProperty(propertyName, propertyValueProvider));
        settings.UseQuota(propertyName, quotaOptionsProvider);
        return settings;
    }
}