using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

/// <summary>
/// Represents additional configuration of <see cref="ThrottlingMiddleware"/>.
/// </summary>
internal class VostokThrottlingConfiguration
{
    /// <inheritdoc cref="IVostokThrottlingBuilder.UseThreadPoolOverloadQuota"/>
    public bool UseThreadPoolOverloadQuota { get; set; } = true;

    /// <summary>
    /// <para>If set to <c>true</c>, sends throttling metrics.</para>
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
}