namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

internal static class SemanticConventions
{
    public const string AttributeHttpRequestContentLength = "http.request.header.content-length";
    public const string AttributeHttpResponseContentLength = "http.response.header.content-length";
    public const string AttributeClientAddress = "client.address";
    public const string AttributeServerAddress = "server.address";
    public const string AttributeServerPort = "server.port";

    public const string AttributeHostName = "host.name";
    public const string AttributeDeploymentEnvironmentName = "deployment.environment.name";

    // Vostok tracing attributes
    public const string HttpClientName = "http.client.name";
}