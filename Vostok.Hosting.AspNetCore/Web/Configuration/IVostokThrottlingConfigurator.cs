using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

/// <summary>
/// Represents <see cref="ThrottlingMiddleware"/> configurator.
/// </summary>
[PublicAPI]
public interface IVostokThrottlingConfigurator
{
    /// <summary>
    /// Allows to customize <see cref="VostokThrottlingConfiguration"/>.
    /// </summary>
    internal IVostokThrottlingConfigurator ConfigureOptions(Action<VostokThrottlingConfiguration> configure);

    /// <inheritdoc cref="IVostokThrottlingBuilder.Metrics"/>
    IVostokThrottlingConfigurator ConfigureMetrics(Action<ThrottlingMetricsOptions> configure);
    
    /// <inheritdoc cref="IVostokThrottlingBuilder.CustomizeMiddleware"/>
    IVostokThrottlingConfigurator ConfigureMiddleware(Action<ThrottlingSettings> configure);

    /// <summary>
    /// Allows to customize <see cref="ThrottlingConfigurationBuilder"/>.
    /// </summary>
    IVostokThrottlingConfigurator ConfigureBuilder(Action<ThrottlingConfigurationBuilder> configure);
}