using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.AspNetCore;
using Vostok.Applications.AspNetCore.OpenTelemetry;
using Vostok.Clusterclient.Core.Model;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

internal static class OpenTelemetryVostokAspNetCoreExtensions
{
    public static void ConfigureAspNetCoreInstrumentationForVostok(this IServiceCollection services) =>
        services.AddOptions<AspNetCoreTraceInstrumentationOptions>()
                .Configure<IServiceBeacon>(Configure);

    private static void Configure(AspNetCoreTraceInstrumentationOptions options, IServiceBeacon beacon)
    {
        VostokContextPropagator.Use();

        var serverAddress = GetAddress(beacon);

        options.EnrichWithHttpRequest += (activity, request) =>
        {
            if (serverAddress is var (host, port))
            {
                activity.SetTag(SemanticConventions.AttributeServerAddress, host);
                activity.SetTag(SemanticConventions.AttributeServerPort, port);
            }

            activity.SetTag(SemanticConventions.AttributeClientAddress, request.HttpContext.Connection.RemoteIpAddress);

            var clientName = request.Headers[HeaderNames.ApplicationIdentity].ToString();
            if (!string.IsNullOrEmpty(clientName))
                activity.SetTag(SemanticConventions.HttpClientName, clientName);

            if (request.ContentLength.HasValue)
                activity.SetTag(SemanticConventions.AttributeHttpRequestContentLength, request.ContentLength.Value);
        };

        options.EnrichWithHttpResponse += (activity, response) =>
        {
            if (response.ContentLength.HasValue)
                activity.SetTag(SemanticConventions.AttributeHttpResponseContentLength, response.ContentLength.Value);
        };
    }

    private static (string host, int port)? GetAddress(IServiceBeacon beacon)
    {
        if (!beacon.ReplicaInfo.TryGetUrl(out var url))
            return null;

        return (url.Host, url.Port);
    }
}