using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.OpenTelemetry.ResourceDetectors;

internal sealed class VostokIdentityResourceDetector(
    IServiceProvider provider,
    VostokIdentityResourceDetector.AttributesOptions? options = null) : IResourceDetector
{
    private readonly IVostokApplicationIdentity? identity = provider.GetService<IVostokApplicationIdentity>();
    private readonly AttributesOptions options = options ?? new AttributesOptions();

    public Resource Detect()
    {
        if (identity is null)
            return Resource.Empty;

        var attributes = new List<KeyValuePair<string, object>>();

        if (options.AddProject)
            attributes.Add(new(WellKnownApplicationIdentityProperties.Project, identity.Project));
        if (options.AddSubproject && identity.Subproject is not null)
            attributes.Add(new(WellKnownApplicationIdentityProperties.Subproject, identity.Subproject));
        if (options.AddEnvironment)
            attributes.Add(new(WellKnownApplicationIdentityProperties.Environment, identity.Environment));
        if (options.AddApplication)
            attributes.Add(new(WellKnownApplicationIdentityProperties.Application, identity.Application));

        if (options.AddInstance)
        {
            var instance = identity.Instance;
            if (string.Equals(instance, EnvironmentInfo.Host, StringComparison.InvariantCultureIgnoreCase))
                instance = instance.ToLowerInvariant();
            attributes.Add(new(WellKnownApplicationIdentityProperties.Instance, instance));
        }

        return new Resource(attributes);
    }

    public class AttributesOptions
    {
        public bool AddProject { get; init; } = true;
        public bool AddSubproject { get; init; } = true;
        public bool AddEnvironment { get; init; } = true;
        public bool AddApplication { get; init; } = true;
        public bool AddInstance { get; init; } = true;
    }
}