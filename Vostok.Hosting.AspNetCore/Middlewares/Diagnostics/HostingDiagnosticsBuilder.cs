using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Middlewares.Diagnostics;

internal sealed class HostingDiagnosticsBuilder : IHostingDiagnosticsBuilder
{
    private readonly IServiceCollection services;

    public HostingDiagnosticsBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public IHostingDiagnosticsBuilder ConfigureMiddleware(Action<DiagnosticApiSettings> configure)
    {
        services.Configure(configure);
        return this;
    }

    public IHostingDiagnosticsBuilder ConfigureFeatures(Action<DiagnosticFeaturesSettings> configure)
    {
        services.Configure(configure);
        return this;
    }
}