using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Builder;

[PublicAPI]
public static class LoggingBuilderExtensions
{
    public static void SetupVostok(this ILoggingBuilder builder, IVostokHostingEnvironment environment)
    {
        var settings = new VostokLoggerProviderSettings();
        settings.AddDefaultLoggingSettings();

        builder.AddVostokLogging(environment, settings);
    }
}