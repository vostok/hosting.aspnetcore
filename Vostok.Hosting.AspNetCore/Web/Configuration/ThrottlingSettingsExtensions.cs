using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class ThrottlingSettingsExtensions
{
    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseConsumerQuota"/>.
    /// </summary>
    public static ThrottlingSettings UseConsumerQuota(this ThrottlingSettings settings)
    {
        settings.AddConsumerProperty = true;
        return settings;
    }

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UsePriorityQuota"/>.
    /// </summary>
    public static ThrottlingSettings UsePriorityQuota(this ThrottlingSettings settings)
    {
        settings.AddPriorityProperty = true;
        return settings;
    }

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseMethodQuota"/>.
    /// </summary>
    public static ThrottlingSettings UseMethodQuota(this ThrottlingSettings settings)
    {
        settings.AddMethodProperty = true;
        return settings;
    }

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseUrlQuota"/>.
    /// </summary>
    public static ThrottlingSettings UseUrlQuota(this ThrottlingSettings settings)
    {
        settings.AddUrlProperty = true;
        return settings;
    }

    /// <summary>
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseCustomPropertyQuota"/>
    /// Also call <see cref="ThrottlingConfigurationBuilderExtensions.UseCustomPropertyQuota"/>.
    /// </summary>
    public static ThrottlingSettings UseCustomPropertyQuota(this ThrottlingSettings settings, string propertyName, Func<HttpContext, string> propertyValueProvider)
    {
        settings.AdditionalProperties.Add(context => (propertyName, propertyValueProvider(context)));
        return settings;
    }
}