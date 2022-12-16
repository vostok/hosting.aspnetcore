using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore;

namespace Vostok.Hosting.AspNetCore.Extensions;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseVostokMiddlewares(this IApplicationBuilder app)
    {
        return app
            .UseVostokHttpContextTweaks()
            .UseVostokRequestInfo()
            .UseVostokDistributedContext()
            .UseVostokTracing()
            .UseVostokThrottling()
            .UseVostokRequestLogging()
            .UseVostokDatacenterAwareness()
            .UseVostokUnhandledExceptions()
            .UseVostokPingApi()
            .UseVostokDiagnosticApi();
    }
}