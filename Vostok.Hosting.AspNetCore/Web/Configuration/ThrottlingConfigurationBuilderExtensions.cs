using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Throttling;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class ThrottlingConfigurationBuilderExtensions
{
    /// <inheritdoc cref="IVostokThrottlingBuilder.UseEssentials"/>
    public static ThrottlingConfigurationBuilder UseEssentials(this ThrottlingConfigurationBuilder builder, Func<ThrottlingEssentials> essentialsProvider) =>
        builder.SetEssentials(essentialsProvider);
    
    /// <inheritdoc cref="IVostokThrottlingBuilder.UsePropertyQuota"/>
    public static ThrottlingConfigurationBuilder UsePropertyQuota(this ThrottlingConfigurationBuilder builder, string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.SetPropertyQuota(propertyName, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilder.UseCustomQuota"/>
    public static ThrottlingConfigurationBuilder UseCustomQuota(this ThrottlingConfigurationBuilder builder, IThrottlingQuota quota) =>
        builder.AddCustomQuota(quota);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/>
    public static ThrottlingConfigurationBuilder UseConsumerQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/>
    public static ThrottlingConfigurationBuilder UsePriorityQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/>
    public static ThrottlingConfigurationBuilder UseMethodQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/>
    public static ThrottlingConfigurationBuilder UseUrlQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseCustomPropertyQuota"/>
    public static ThrottlingConfigurationBuilder UseCustomPropertyQuota(this ThrottlingConfigurationBuilder builder, string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.SetPropertyQuota(propertyName, quotaOptionsProvider);
}