using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore;
using Vostok.Hosting.AspNetCore.Builders.Middlewares;

namespace Vostok.Hosting.AspNetCore.Extensions;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseVostokMiddlewares(this IApplicationBuilder app)
    {
        var settings = app.ApplicationServices.GetFromOptionsOrDefault<EnabledVostokMiddlewaresSettings>();

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