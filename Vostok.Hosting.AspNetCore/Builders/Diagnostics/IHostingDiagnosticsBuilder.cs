using System;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Builders.Diagnostics;

public interface IHostingDiagnosticsBuilder
{
    IHostingDiagnosticsBuilder ConfigureMiddleware(Action<DiagnosticApiSettings> configure);
    IHostingDiagnosticsBuilder ConfigureFeatures(Action<DiagnosticFeaturesSettings> configure);
}