using ConsoleApp1;
using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

var builder = Host.CreateDefaultBuilder(args);

builder.UseVostokHosting(SetupVostok);

builder.ConfigureServices(services => { services.AddHostedService<Worker>(); });

var app = builder.Build();

app.Run();

void SetupVostok(IVostokHostingEnvironmentBuilder builder)
{
    builder.SetupApplicationIdentity(identity =>
    {
        identity.SetProject("Vostok");
        identity.SetSubproject("Test");
        identity.SetApplication("AspNetCoreHostingConsole");
        identity.SetEnvironment("dev");
        identity.SetInstance("0");
    });

    builder.SetupLog(log =>
    {
        log.SetupConsoleLog();
        log.SetupFileLog(fileLog => fileLog.CustomizeSettings(
            fileLogSettings => fileLogSettings.FileOpenMode = FileOpenMode.Rewrite));
    });
}