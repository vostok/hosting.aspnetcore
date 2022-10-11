using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Aspnetcore.Builder;

[PublicAPI]
public static class ConfigurationBuilderExtensions
{
    public static void SetupVostok(this IConfigurationBuilder builder, IVostokHostingEnvironment environment)
    {
        builder.AddVostokSources(environment);
        builder.AddDefaultLoggingFilters();
    }
}