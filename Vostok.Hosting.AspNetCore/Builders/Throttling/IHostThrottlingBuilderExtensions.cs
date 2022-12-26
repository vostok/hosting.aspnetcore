using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Throttling;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Builders.Throttling;

[PublicAPI]
public static class IHostThrottlingBuilderExtensions
{
    /// <summary>
    /// Disables the throttling middleware altogether.
    /// </summary>
    public static IHostingThrottlingBuilder DisableThrottling(this IHostingThrottlingBuilder builder)
        => builder.ConfigureMiddleware(s => s.Enabled = _ => false);

    /// <summary>
    /// <para>Sets up a quota on the <see cref="WellKnownThrottlingProperties.Consumer"/> request property configured by given <paramref name="quotaOptionsProvider"/>.</para>
    /// <para>See <see cref="IHostingThrottlingBuilder.UsePropertyQuota"/> for additional info on property quotas.</para>
    /// </summary>
    public static IHostingThrottlingBuilder UseConsumerQuota(this IHostingThrottlingBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder
            .ConfigureMiddleware(settings => settings.AddConsumerProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Consumer, quotaOptionsProvider);

    /// <summary>
    /// <para>Sets up a quota on the <see cref="WellKnownThrottlingProperties.Priority"/> request property configured by given <paramref name="quotaOptionsProvider"/>.</para>
    /// <para>See <see cref="IHostingThrottlingBuilder.UsePropertyQuota"/> for additional info on property quotas.</para>
    /// </summary>
    public static IHostingThrottlingBuilder UsePriorityQuota(this IHostingThrottlingBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder
            .ConfigureMiddleware(settings => settings.AddPriorityProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Priority, quotaOptionsProvider);

    /// <summary>
    /// <para>Sets up a quota on the <see cref="WellKnownThrottlingProperties.Method"/> request property configured by given <paramref name="quotaOptionsProvider"/>.</para>
    /// <para>See <see cref="IHostingThrottlingBuilder.UsePropertyQuota"/> for additional info on property quotas.</para>
    /// </summary>
    public static IHostingThrottlingBuilder UseMethodQuota(this IHostingThrottlingBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder
            .ConfigureMiddleware(settings => settings.AddMethodProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Method, quotaOptionsProvider);

    /// <summary>
    /// <para>Sets up a quota on the <see cref="WellKnownThrottlingProperties.Url"/> request property configured by given <paramref name="quotaOptionsProvider"/>.</para>
    /// <para>See <see cref="IHostingThrottlingBuilder.UsePropertyQuota"/> for additional info on property quotas.</para>
    /// </summary>
    public static IHostingThrottlingBuilder UseUrlQuota(this IHostingThrottlingBuilder builder, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder
            .ConfigureMiddleware(settings => settings.AddUrlProperty = true)
            .UsePropertyQuota(WellKnownThrottlingProperties.Url, quotaOptionsProvider);

    /// <summary>
    /// <para>Sets up a quota on the <paramref name="propertyName"/> property configured by given <paramref name="quotaOptionsProvider"/>.</para>
    /// <para>Property value will be obtained from <paramref name="propertyValueProvider"/>.</para>
    /// <para>See <see cref="IHostingThrottlingBuilder.UsePropertyQuota"/> for additional info on property quotas.</para>
    /// </summary>
    public static IHostingThrottlingBuilder UseCustomPropertyQuota(this IHostingThrottlingBuilder builder, string propertyName, Func<HttpContext, string> propertyValueProvider, Func<PropertyQuotaOptions> quotaOptionsProvider) =>
        builder
            .ConfigureMiddleware(settings => settings.AdditionalProperties.Add(context => (propertyName, propertyValueProvider(context))))
            .UsePropertyQuota(propertyName, quotaOptionsProvider);
}