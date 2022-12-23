using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Builders;
using Vostok.Hosting.AspNetCore.MiddlewareRegistration;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public static class IVostokMiddlewaresBuilderExtensions
{
    public static IVostokMiddlewaresBuilder ConfigureHttpContextTweaks(
        this IVostokMiddlewaresBuilder builder,
        Action<HttpContextTweakSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureRequestInfoFilling(
        this IVostokMiddlewaresBuilder builder,
        Action<FillRequestInfoSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureDistributedContext(
        this IVostokMiddlewaresBuilder builder,
        Action<DistributedContextSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureTracing(
        this IVostokMiddlewaresBuilder builder,
        Action<TracingSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureThrottling(
        this IVostokMiddlewaresBuilder builder,
        Action<IHostingThrottlingBuilder> configure)
    {
        configure(new HostingThrottlingBuilder(builder.Services));

        return builder;
    }

    public static IVostokMiddlewaresBuilder ConfigureRequestLogging(
        this IVostokMiddlewaresBuilder builder,
        Action<LoggingSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureDatacenterAwareness(
        this IVostokMiddlewaresBuilder builder,
        Action<DatacenterAwarenessSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureUnhandledExceptions(
        this IVostokMiddlewaresBuilder builder,
        Action<UnhandledExceptionSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigurePingApi(
        this IVostokMiddlewaresBuilder builder,
        Action<PingApiSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureDiagnosticApi(
        this IVostokMiddlewaresBuilder builder,
        Action<DiagnosticApiSettings> configure)
    {
        return builder.Configure(configure);
    }

    public static IVostokMiddlewaresBuilder ConfigureDiagnosticFeatures(
        this IVostokMiddlewaresBuilder builder,
        Action<DiagnosticFeaturesSettings> configure)
    {
        return builder.Configure(configure);
    }

    private static IVostokMiddlewaresBuilder Configure<T>(this IVostokMiddlewaresBuilder builder, Action<T> configure)
        where T : class
    {
        builder.Services.Configure(configure);
        return builder;
    }
}