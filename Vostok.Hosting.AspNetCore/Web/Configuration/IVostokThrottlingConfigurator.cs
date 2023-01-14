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
    /// <inheritdoc cref="IVostokThrottlingBuilder.UseThreadPoolOverloadQuota"/>
    bool UseThreadPoolOverloadQuota { set; }
    
    internal bool EnableMetrics { set; }
    
    /// <inheritdoc cref="IVostokThrottlingBuilder.Metrics"/>
    IVostokThrottlingConfigurator ConfigureMetrics(Action<ThrottlingMetricsOptions> configure);
    
    /// <inheritdoc cref="IVostokThrottlingBuilder.UseEssentials"/>
    IVostokThrottlingConfigurator UseEssentials(Func<ThrottlingEssentials> essentialsProvider);
    
    /// <inheritdoc cref="IVostokThrottlingBuilder.UsePropertyQuota"/>
    IVostokThrottlingConfigurator UsePropertyQuota(string propertyName, Func<PropertyQuotaOptions> quotaOptionsProvider);
    
    /// <inheritdoc cref="IVostokThrottlingBuilder.UseCustomQuota"/>
    IVostokThrottlingConfigurator UseCustomQuota(IThrottlingQuota quota);
    
    /// <inheritdoc cref="IVostokThrottlingBuilder.CustomizeMiddleware"/>
    IVostokThrottlingConfigurator ConfigureMiddleware(Action<ThrottlingSettings> configure);
}