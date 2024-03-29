﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Web.Configuration;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Web;

/// <summary>
/// <para>Adds Vostok middlewares to the specified application builder.</para>
/// </summary>
[PublicAPI]
public static class UseVostokMiddlewaresExtensions
{
    private const string Slash = "/";

    /// <inheritdoc cref="UseVostokMiddlewaresExtensions"/>
    public static IApplicationBuilder UseVostokMiddlewares(this IApplicationBuilder applicationBuilder)
    {
        var settings = applicationBuilder.ApplicationServices.GetFromOptionsOrThrow<VostokMiddlewaresConfiguration>();
        settings.MiddlewaresAdded = true;
        
        var middlewares = new List<Type>();

        applicationBuilder.UseBeaconPathBase();

        Add<HttpContextTweakMiddleware>();
        Add<FillRequestInfoMiddleware>();
        Add<DistributedContextMiddleware>();
        Add<TracingMiddleware>();
        Add<ThrottlingMiddleware>();
        Add<LoggingMiddleware>();
        Add<DatacenterAwarenessMiddleware>();
        Add<UnhandledExceptionMiddleware>();
        Add<PingApiMiddleware>();
        Add<DiagnosticApiMiddleware>();

        foreach (var middleware in middlewares)
            applicationBuilder.UseMiddleware(middleware);

        return applicationBuilder;

        void Add<TMiddleware>()
        {
            if (settings.PreVostokMiddlewares.TryGetValue(typeof(TMiddleware), out var injected))
                middlewares.AddRange(injected);

            if (settings.IsEnabled<TMiddleware>())
            {
                middlewares.Add(typeof(TMiddleware));
            }
        }
    }

    private static void UseBeaconPathBase(this IApplicationBuilder applicationBuilder)
    {
        var serviceBeacon = applicationBuilder.ApplicationServices.GetRequiredService<IServiceBeacon>();
        if (!serviceBeacon.ReplicaInfo.TryGetUrl(out var url))
            return;

        var urlPath = url.AbsolutePath;
        if (string.IsNullOrEmpty(urlPath) || urlPath == Slash)
            return;

        applicationBuilder.UsePathBase(urlPath);
    }
}