using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class ThrottlingSettingsExtensions
{
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/>
    public static ThrottlingSettings UseConsumerQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UsePropertyQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/>
    public static ThrottlingSettings UsePriorityQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UsePropertyQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/>
    public static ThrottlingSettings UseMethodQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UsePropertyQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/>
    public static ThrottlingSettings UseUrlQuota(this ThrottlingSettings settings, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        settings.UsePropertyQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilder.UseEssentials"/>
    public static ThrottlingSettings UseEssentials(this ThrottlingSettings settings, Func<ThrottlingEssentials> essentialsProvider)
    {
        settings.Essentials = essentialsProvider;
        return settings;
    }

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseCustomPropertyQuota"/>
    public static ThrottlingSettings UseCustomPropertyQuota(this ThrottlingSettings settings, string propertyName, Func<HttpContext, string> propertyValueProvider, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        settings.Properties.Add(new ThrottlingProperty(propertyName, propertyValueProvider));
        settings.UsePropertyQuota(propertyName, quotaOptionsProvider);
        return settings;
    }

    private static ThrottlingSettings UsePropertyQuota(this ThrottlingSettings settings, string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider)
    {
        settings.Quotas.Add(new ThrottlingQuota(propertyName, quotaOptionsProvider));
        return settings;
    }
}