using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016")]
public sealed class VostokOpenTelemetryMeterResourceOptions
{
    public bool AddProject { get; set; } = true;
    public bool AddSubproject { get; set; } = true;
    public bool AddEnvironment { get; set; } = true;
    public bool AddApplication { get; set; } = true;
    public bool AddInstance { get; set; } = true;
}