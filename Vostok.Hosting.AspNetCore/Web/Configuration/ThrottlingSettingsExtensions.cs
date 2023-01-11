using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class ThrottlingSettingsExtensions
{
    public static ThrottlingSettings UseQuota(this ThrottlingSettings settings, string name, Func<PropertyQuotaOptions> optionsProvider)
    {
        settings.Quotas.Add(new ThrottlingQuota(name, optionsProvider));
        return settings;
    }

    public static ThrottlingSettings UseConsumerQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> optionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Consumer, optionsProvider);

    public static ThrottlingSettings UsePriorityQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> optionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Priority, optionsProvider);

    public static ThrottlingSettings UseMethodQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> optionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Method, optionsProvider);

    public static ThrottlingSettings UseUrlQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> optionsProvider) =>
        settings.UseQuota(WellKnownThrottlingProperties.Url, optionsProvider);

    public static ThrottlingSettings UseEssentials(this ThrottlingSettings settings, Func<ThrottlingEssentials> optionsProvider)
    {
        settings.Essentials = optionsProvider;
        return settings;
    }

    public static ThrottlingSettings UseEssentials(this ThrottlingSettings settings, ThrottlingEssentials essentials) =>
        settings.UseEssentials(() => essentials);

    public static ThrottlingSettings UseCustomPropertyQuota(this ThrottlingSettings settings, string name, Func<HttpContext, string> valueProvider, Func<PropertyQuotaOptions> optionsProvider)
    {
        settings.Properties.Add(new ThrottlingProperty(name, valueProvider));
        settings.UseQuota(name, optionsProvider);
        return settings;
    }
}