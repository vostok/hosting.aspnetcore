using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

/// <summary>
/// Represents Vostok middlewares configurator.
/// </summary>
[PublicAPI]
public interface IVostokMiddlewaresConfigurator
{
    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupHttpContextTweaks"/>
    IVostokMiddlewaresConfigurator ConfigureHttpContextTweaks(Action<HttpContextTweakSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupRequestInfoFilling"/>
    IVostokMiddlewaresConfigurator ConfigureRequestInfoFilling(Action<FillRequestInfoSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDistributedContext"/>
    IVostokMiddlewaresConfigurator ConfigureDistributedContext(Action<DistributedContextSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupTracing"/>
    IVostokMiddlewaresConfigurator ConfigureTracing(Action<TracingSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupThrottling"/>
    IVostokMiddlewaresConfigurator ConfigureThrottling(Action<IVostokThrottlingConfigurator> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupLogging"/>
    IVostokMiddlewaresConfigurator ConfigureLogging(Action<LoggingSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDatacenterAwareness"/>
    IVostokMiddlewaresConfigurator ConfigureDatacenterAwareness(Action<DatacenterAwarenessSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupUnhandledExceptions"/>
    IVostokMiddlewaresConfigurator ConfigureUnhandledExceptions(Action<UnhandledExceptionSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupPingApi"/>
    IVostokMiddlewaresConfigurator ConfigurePingApi(Action<PingApiSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDiagnosticApi"/>
    IVostokMiddlewaresConfigurator ConfigureDiagnosticApi(Action<DiagnosticApiSettings> configure);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.SetupDiagnosticFeatures"/>
    IVostokMiddlewaresConfigurator ConfigureDiagnosticFeatures(Action<DiagnosticFeaturesSettings> configure);

    /// <summary>
    /// Allows to customize <see cref="VostokMiddlewaresConfiguration"/>.
    /// </summary>
    internal IVostokMiddlewaresConfigurator ConfigureOptions(Action<VostokMiddlewaresConfiguration> configure);
}