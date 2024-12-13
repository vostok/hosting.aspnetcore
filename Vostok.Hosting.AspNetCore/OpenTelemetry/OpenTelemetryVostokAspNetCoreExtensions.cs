using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AspNetCore;
using Vostok.Applications.AspNetCore.OpenTelemetry;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016")]
public static class OpenTelemetryVostokAspNetCoreExtensions
{
    public static IServiceCollection ConfigureAspNetCoreInstrumentationForVostok(
        this IServiceCollection serviceCollection,
        string? name = null)
    {
        VostokContextPropagator.Use();

        name ??= Options.DefaultName;
        serviceCollection.Configure<AspNetCoreTraceInstrumentationOptions>(name, ConfigureOptions);
        return serviceCollection;

        static void ConfigureOptions(AspNetCoreTraceInstrumentationOptions options)
        {
            options.EnrichWithHttpRequest += (activity, request) =>
            {
                activity.SetTag(SemanticConventions.AttributeClientAddress, request.HttpContext.Connection.RemoteIpAddress);

                var clientName = request.Headers[HeaderNames.ApplicationIdentity].ToString();
                if (!string.IsNullOrEmpty(clientName))
                    activity.SetTag("http.client.name", clientName);

                if (request.ContentLength.HasValue)
                    activity.SetTag(SemanticConventions.AttributeHttpRequestContentLength, request.ContentLength.Value);
            };

            options.EnrichWithHttpResponse += (activity, response) =>
            {
                if (response.ContentLength.HasValue)
                    activity.SetTag(SemanticConventions.AttributeHttpResponseContentLength, response.ContentLength.Value);
            };
        }
    }
}