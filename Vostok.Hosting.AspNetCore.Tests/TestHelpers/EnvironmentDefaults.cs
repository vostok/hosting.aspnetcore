using Vostok.Configuration.Sources.Object;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

namespace Vostok.Hosting.AspNetCore.Tests.TestHelpers;

internal static class EnvironmentDefaults
{
    public static void ApplyTestsDefaults(this IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupApplicationIdentity(identity =>
        {
            identity.SetProject("Vostok");
            identity.SetSubproject("Test");
            identity.SetApplication("SomeApplication");
            identity.SetEnvironment("dev");
            identity.SetInstance("1");
        });

        builder.SetupLog(log =>
        {
            log.SetupConsoleLog();
            log.SetupFileLog(fileLog => fileLog.CustomizeSettings(
                fileLogSettings => fileLogSettings.FileOpenMode = FileOpenMode.Rewrite));
        });

        builder.SetupConfiguration(config =>
        {
            config.AddSource(new ObjectSource(new
            {
                Logging = new
                {
                    LogLevel = new
                    {
                        Default = "Trace",
                        Microsoft = "Trace"
                    }
                }
            }));
        });
    }
}