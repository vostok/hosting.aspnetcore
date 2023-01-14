using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Web.Configuration;

namespace Vostok.Hosting.AspNetCore.Web;

/// <summary>
/// <para>Adds Vostok middlewares to the specified application builder.</para>
/// </summary>
[PublicAPI]
public static class UseVostokMiddlewaresExtensions
{
    /// <inheritdoc cref="UseVostokMiddlewaresExtensions"/>
    public static IApplicationBuilder UseVostokMiddlewares(this IApplicationBuilder applicationBuilder)
    {
        var settings = applicationBuilder.ApplicationServices.GetFromOptionsOrThrow<VostokMiddlewaresConfiguration>();
        var middlewares = new List<Type>();
        
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
}