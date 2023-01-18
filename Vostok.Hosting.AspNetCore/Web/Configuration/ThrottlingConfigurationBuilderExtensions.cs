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

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilder.UsePropertyQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UsePropertyQuota"/>.
    /// </summary>
    public static ThrottlingConfigurationBuilder UsePropertyQuota(this ThrottlingConfigurationBuilder builder, string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.SetPropertyQuota(propertyName, quotaOptionsProvider);

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilder.UseCustomQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseCustomQuota"/>.
    /// </summary>
    public static ThrottlingConfigurationBuilder UseCustomQuota(this ThrottlingConfigurationBuilder builder, IThrottlingQuota quota) =>
        builder.AddCustomQuota(quota);

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseConsumerQuota"/>.
    /// </summary>
    public static ThrottlingConfigurationBuilder UseConsumerQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider);

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UsePriorityQuota"/>.
    /// </summary>
    public static ThrottlingConfigurationBuilder UsePriorityQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider);

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseMethodQuota"/>.
    /// </summary>
    public static ThrottlingConfigurationBuilder UseMethodQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider);

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseUrlQuota"/>.
    /// </summary>
    public static ThrottlingConfigurationBuilder UseUrlQuota(this ThrottlingConfigurationBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.UsePropertyQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider);

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseCustomPropertyQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseCustomPropertyQuota"/>.
    /// </summary>
    public static ThrottlingConfigurationBuilder UseCustomPropertyQuota(this ThrottlingConfigurationBuilder builder, string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder.SetPropertyQuota(propertyName, quotaOptionsProvider);
}