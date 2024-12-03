using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Vostok.Applications.AspNetCore.OpenTelemetry;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureOpenTelemetryTracerProviderForVostok(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureOpenTelemetryTracerProvider((services, tracing) =>
            tracing.ConfigureResource(resourceBuilder =>
                resourceBuilder.Clear()
                               .AddTelemetrySdk()
                               .ConfigureVostokTracerResource(services)));

        return serviceCollection;
    }

    public static IServiceCollection ConfigureOpenTelemetryMeterProviderForVostok(
        this IServiceCollection serviceCollection,
        Action<VostokOpenTelemetryMeterProviderOptions>? configure = null)
    {
        if (configure != null)
            serviceCollection.Configure(configure);

        serviceCollection.ConfigureOpenTelemetryMeterProvider((services, metrics) =>
            metrics.ConfigureResource(resourceBuilder =>
                resourceBuilder.Clear()
                               .ConfigureMeterResource(services)));

        return serviceCollection;
    }

    public static IServiceCollection ConfigureOpenTelemetryLoggingProviderForVostok(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureOpenTelemetryLoggerProvider((services, logging) =>
            logging.ConfigureResource(resourceBuilder =>
                resourceBuilder.Clear()
                               .AddTelemetrySdk()
                               .ConfigureLoggingResource(services)));

        return serviceCollection;
    }

    private static void ConfigureVostokTracerResource(this ResourceBuilder resourceBuilder, IServiceProvider services)
    {
        var host = EnvironmentInfo.Host;

        // todo (ponomaryovigor, 03.12.2024):  ClusterClientDefaults.ClientApplicationName;
        var application = EnvironmentInfo.Application;
        string? environment = null;

        var serviceBeacon = services.GetService<IServiceBeacon>();
        if (serviceBeacon != null)
        {
            application = serviceBeacon.ReplicaInfo.Application;
            environment = serviceBeacon.ReplicaInfo.Environment;
        }

        resourceBuilder.AddService(application, autoGenerateServiceInstanceId: false);
        List<KeyValuePair<string, object>> vostokTags = [new(SemanticConventions.AttributeHostName, host)];
        if (environment != null)
            vostokTags.Add(new(SemanticConventions.AttributeDeploymentEnvironmentName, environment));

        resourceBuilder.AddAttributes(vostokTags);
    }

    private static void ConfigureMeterResource(this ResourceBuilder resourceBuilder, IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptionsMonitor<VostokOpenTelemetryMeterProviderOptions>>().CurrentValue;

        var identity = services.GetRequiredService<IVostokApplicationIdentity>();
        var vostokTags = new List<KeyValuePair<string, object>>();
        if (options.AddProject)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Project, identity.Project));
        if (options.AddSubproject && identity.Subproject != null)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Subproject, identity.Subproject));
        if (options.AddEnvironment)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Environment, identity.Environment));
        if (options.AddApplication)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Application, identity.Application));

        if (options.AddInstance)
        {
            var instance = identity.Instance;
            if (string.Equals(instance, EnvironmentInfo.Host, StringComparison.InvariantCultureIgnoreCase))
                instance = instance.ToLowerInvariant();
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Instance, instance));
        }

        resourceBuilder.AddAttributes(vostokTags);
    }

    private static void ConfigureLoggingResource(this ResourceBuilder resourceBuilder, IServiceProvider services)
    {
        var host = EnvironmentInfo.Host;
        
        // todo (ponomaryovigor, 03.12.2024):  ClusterClientDefaults.ClientApplicationName;
        var application = EnvironmentInfo.Application;
        string? environment = null;

        var serviceBeacon = services.GetService<IServiceBeacon>();
        if (serviceBeacon != null)
        {
            application = serviceBeacon.ReplicaInfo.Application;
            environment = serviceBeacon.ReplicaInfo.Environment;
        }

        resourceBuilder.AddService(application, autoGenerateServiceInstanceId: false);
        var vostokTags = new List<KeyValuePair<string, object>>
        {
            new(SemanticConventions.AttributeHostName, host)
        };
        if (environment != null)
            vostokTags.Add(new(SemanticConventions.AttributeDeploymentEnvironmentName, environment));

        var identity = services.GetRequiredService<IVostokApplicationIdentity>();

        vostokTags.Add(new(WellKnownApplicationIdentityProperties.Project, identity.Project));
        if (identity.Subproject is {} subproject)
            vostokTags.Add(new(WellKnownApplicationIdentityProperties.Subproject, subproject));
        vostokTags.Add(new(WellKnownApplicationIdentityProperties.Environment, identity.Environment));
        vostokTags.Add(new(WellKnownApplicationIdentityProperties.Application, identity.Application));

        var instance = identity.Instance;
        if (string.Equals(instance, EnvironmentInfo.Host, StringComparison.InvariantCultureIgnoreCase))
            instance = instance.ToLowerInvariant();
        vostokTags.Add(new(WellKnownApplicationIdentityProperties.Instance, instance));

        resourceBuilder.AddAttributes(vostokTags);
    }
}