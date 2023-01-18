using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Throttling.Config;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class IVostokThrottlingConfiguratorExtensions
{
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.DisableThrottling"/>
    public static IVostokThrottlingConfigurator DisableThrottling(this IVostokThrottlingConfigurator configurator) =>
        configurator.ConfigureMiddleware(settings => settings.Enabled = _ => false);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.DisableMetrics"/>
    public static IVostokThrottlingConfigurator DisableMetrics(this IVostokThrottlingConfigurator configurator) =>
        configurator.ConfigureOptions(s => s.EnableMetrics = false);

    /// <inheritdoc cref="IVostokThrottlingBuilder.UseThreadPoolOverloadQuota"/>
    public static IVostokThrottlingConfigurator DisableThreadPoolOverloadQuota(this IVostokThrottlingConfigurator configurator) =>
        configurator.ConfigureOptions(s => s.UseThreadPoolOverloadQuota = false);

    /// <inheritdoc cref="IVostokThrottlingBuilder.UseEssentials"/>
    public static IVostokThrottlingConfigurator UseEssentials(this IVostokThrottlingConfigurator configurator, Func<ThrottlingEssentials> essentialsProvider) =>
        configurator.ConfigureBuilder(builder => builder.UseEssentials(essentialsProvider));

    /// <inheritdoc cref="IVostokThrottlingBuilder.UsePropertyQuota"/>
    public static IVostokThrottlingConfigurator UsePropertyQuota(this IVostokThrottlingConfigurator configurator, string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator.ConfigureBuilder(builder => builder.UsePropertyQuota(propertyName, quotaOptionsProvider));

    /// <inheritdoc cref="IVostokThrottlingBuilder.UseCustomQuota"/>
    public static IVostokThrottlingConfigurator UseCustomQuota(this IVostokThrottlingConfigurator configurator, IThrottlingQuota quota) =>
        configurator.ConfigureBuilder(builder => builder.UseCustomQuota(quota));

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/>
    public static IVostokThrottlingConfigurator UseConsumerQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.UseConsumerQuota())
            .ConfigureBuilder(builder => builder.UseConsumerQuota(quotaOptionsProvider));

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/>
    public static IVostokThrottlingConfigurator UsePriorityQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.UsePriorityQuota())
            .ConfigureBuilder(builder => builder.UsePriorityQuota(quotaOptionsProvider));

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/>
    public static IVostokThrottlingConfigurator UseMethodQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.UseMethodQuota())
            .ConfigureBuilder(builder => builder.UseMethodQuota(quotaOptionsProvider));

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/>
    public static IVostokThrottlingConfigurator UseUrlQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.UseUrlQuota())
            .ConfigureBuilder(builder => builder.UseUrlQuota(quotaOptionsProvider));

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseCustomPropertyQuota"/>
    public static IVostokThrottlingConfigurator UseCustomPropertyQuota(this IVostokThrottlingConfigurator configurator, string propertyName, Func<HttpContext, string> propertyValueProvider, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.UseCustomPropertyQuota(propertyName, propertyValueProvider))
            .ConfigureBuilder(builder => builder.UseCustomPropertyQuota(propertyName, quotaOptionsProvider));
}