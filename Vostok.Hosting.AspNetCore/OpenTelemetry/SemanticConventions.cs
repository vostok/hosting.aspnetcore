namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

internal static class SemanticConventions
{
    public const string AttributeHttpRequestContentLength = "http.request.header.content-length";
    public const string AttributeHttpResponseContentLength = "http.response.header.content-length";
    public const string AttributeClientAddress = "client.address";
    public const string AttributePeerService = "peer.service";

    public const string AttributeHostName = "host.name";
    public const string AttributeDeploymentEnvironmentName = "deployment.environment.name";
}