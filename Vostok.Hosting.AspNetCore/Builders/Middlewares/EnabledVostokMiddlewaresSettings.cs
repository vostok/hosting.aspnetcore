namespace Vostok.Hosting.AspNetCore.Builders.Middlewares;

public class EnabledVostokMiddlewaresSettings
{
    public bool EnableHttpContextTweaks { get; set; } = true;
    public bool EnableRequestInfoFilling { get; set; } = true;
    public bool EnableDistributedContext { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public bool EnableThrottling { get; set; } = true;
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableDatacenterAwareness { get; set; } = true;
    public bool EnableUnhandledExceptionsHandling { get; set; } = true;
    public bool EnablePingApi { get; set; } = true;
    public bool EnableDiagnosticApi { get; set; } = true;
}