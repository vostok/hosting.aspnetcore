using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public static class IVostokMiddlewaresConfiguratorExtensions
{
    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.DisableVostokMiddleware{TMiddleware}"/>
    public static IVostokMiddlewaresConfigurator DisableVostokMiddleware<TMiddleware>(this IVostokMiddlewaresConfigurator configurator) =>
        configurator.ConfigureOptions(config => config.MiddlewareDisabled[typeof(TMiddleware)] = true);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.EnableVostokMiddleware{TMiddleware}"/>
    public static IVostokMiddlewaresConfigurator EnableVostokMiddleware<TMiddleware>(this IVostokMiddlewaresConfigurator configurator) =>
        configurator.ConfigureOptions(config => config.MiddlewareDisabled[typeof(TMiddleware)] = false);

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.InjectPreVostokMiddleware{TMiddleware,TBefore}"/>
    public static IVostokMiddlewaresConfigurator InjectPreVostokMiddleware<TMiddleware>(this IVostokMiddlewaresConfigurator configurator) =>
        configurator.InjectPreVostokMiddleware<TMiddleware, FillRequestInfoMiddleware>();

    /// <inheritdoc cref="IVostokAspNetCoreApplicationBuilder.InjectPreVostokMiddleware{TMiddleware,TBefore}"/>
    public static IVostokMiddlewaresConfigurator InjectPreVostokMiddleware<TMiddleware, TBefore>(this IVostokMiddlewaresConfigurator configurator) =>
        configurator.ConfigureOptions(config =>
        {
            if (!config.PreVostokMiddlewares.TryGetValue(typeof(TBefore), out var injected))
                config.PreVostokMiddlewares[typeof(TBefore)] = injected = new List<Type>();

            injected.Add(typeof(TMiddleware));
        });
}