using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using Vostok.Commons.Environment;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry.ResourceDetectors;

internal sealed class ServiceResourceDetector(IServiceProvider provider) : IResourceDetector
{
    private readonly IServiceBeacon? beacon = provider.GetService<IServiceBeacon>();

    public Resource Detect()
    {
        // todo (ponomaryovigor, 03.12.2024):  ClusterClientDefaults.ClientApplicationName;
        var service = EnvironmentInfo.Application;
        string? environment = null;

        if (beacon != null)
        {
            service = beacon.ReplicaInfo.Application;
            environment = beacon.ReplicaInfo.Environment;
        }

        List<KeyValuePair<string, object>> attributes = [new(SemanticConventions.AttributeHostName, EnvironmentInfo.Host)];
        if (environment != null)
            attributes.Add(new(SemanticConventions.AttributeDeploymentEnvironmentName, environment));

        return ResourceBuilder.CreateEmpty()
                              .AddService(service, autoGenerateServiceInstanceId: false)
                              .AddAttributes(attributes)
                              .Build();
    }
}