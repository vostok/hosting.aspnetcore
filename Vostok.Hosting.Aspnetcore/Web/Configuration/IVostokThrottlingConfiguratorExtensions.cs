﻿using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Throttling;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class IVostokThrottlingConfiguratorExtensions
{
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.DisableThrottling"/>
    public static IVostokThrottlingConfigurator DisableThrottling(this IVostokThrottlingConfigurator configurator)
        => configurator.ConfigureMiddleware(s => s.Enabled = _ => false);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.DisableMetrics"/>
    public static IVostokThrottlingConfigurator DisableMetrics(this IVostokThrottlingConfigurator configurator)
    {
        configurator.EnableMetrics = false;
        return configurator;
    }

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/>
    public static IVostokThrottlingConfigurator UseConsumerQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.AddConsumerProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/>
    public static IVostokThrottlingConfigurator UsePriorityQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.AddPriorityProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/>
    public static IVostokThrottlingConfigurator UseMethodQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.AddMethodProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/>
    public static IVostokThrottlingConfigurator UseUrlQuota(this IVostokThrottlingConfigurator configurator, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.AddUrlProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider);

    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseCustomPropertyQuota"/>
    public static IVostokThrottlingConfigurator UseCustomPropertyQuota(this IVostokThrottlingConfigurator configurator, string propertyName, Func<HttpContext, string> propertyValueProvider, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        configurator
            .ConfigureMiddleware(settings => settings.AdditionalProperties.Add(context => (propertyName, propertyValueProvider(context))))
            .UsePropertyQuota(propertyName, quotaOptionsProvider);
}