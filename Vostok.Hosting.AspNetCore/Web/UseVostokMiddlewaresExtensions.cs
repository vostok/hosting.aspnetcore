using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore;
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
    public static IApplicationBuilder UseVostokMiddlewares(this IApplicationBuilder app)
    {
        var settings = app.ApplicationServices.GetFromOptionsOrDefault<VostokMiddlewaresEnabledSettings>();

        if (settings.EnableHttpContextTweaks)
            app.UseVostokHttpContextTweaks();

        if (settings.EnableRequestInfoFilling)
            app.UseVostokRequestInfo();

        if (settings.EnableDistributedContext)
            app.UseVostokDistributedContext();

        if (settings.EnableTracing)
            app.UseVostokTracing();

        if (settings.EnableThrottling)
            app.UseVostokThrottling();

        if (settings.EnableRequestLogging)
            app.UseVostokRequestLogging();

        if (settings.EnableDatacenterAwareness)
            app.UseVostokDatacenterAwareness();

        if (settings.EnableUnhandledExceptionsHandling)
            app.UseVostokUnhandledExceptions();

        if (settings.EnablePingApi)
            app.UseVostokPingApi();

        if (settings.EnableDiagnosticApi)
            app.UseVostokDiagnosticApi();

        return app;
    }
}