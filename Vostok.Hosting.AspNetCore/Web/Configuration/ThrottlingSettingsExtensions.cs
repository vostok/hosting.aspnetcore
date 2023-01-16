using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class ThrottlingSettingsExtensions
{
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseConsumerQuota"/>
    public static ThrottlingSettings UseConsumerQuota(this ThrottlingSettings settings)
    {
        settings.AddConsumerProperty = true;
        return settings;
    }
    
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UsePriorityQuota"/>
    public static ThrottlingSettings UsePriorityQuota(this ThrottlingSettings settings)
    {
        settings.AddPriorityProperty = true;
        return settings;
    }
    
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseMethodQuota"/>
    public static ThrottlingSettings UseMethodQuota(this ThrottlingSettings settings)
    {
        settings.AddMethodProperty = true;
        return settings;
    }
    
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseUrlQuota"/>
    public static ThrottlingSettings UseUrlQuota(this ThrottlingSettings settings)
    {
        settings.AddUrlProperty = true;
        return settings;
    }
    
    /// <inheritdoc cref="IVostokThrottlingBuilderExtensions.UseCustomPropertyQuota"/>
    public static ThrottlingSettings UseCustomPropertyQuota(this ThrottlingSettings settings, string propertyName, Func<HttpContext, string> propertyValueProvider)
    {
        settings.AdditionalProperties.Add(context => (propertyName, propertyValueProvider(context)));
        return settings;
    }
}