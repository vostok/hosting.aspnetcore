using Microsoft.Extensions.Configuration;
using Vostok.Configuration.Microsoft;
using Vostok.Configuration.Sources.Object;

namespace Vostok.Hosting.AspNetCore.Extensions;

internal static class IConfigurationBuilderExtensions
{
    public static void AddDefaultLoggingFilters(this IConfigurationBuilder builder)
    {
        builder.Sources.Insert(0,
            new VostokConfigurationSource(new ObjectSource(new
            {
                Logging = new
                {
                    LogLevel = new
                    {
                        Default = "Information",
                        Microsoft = "Warning"
                    }
                }
            })));
    }
}