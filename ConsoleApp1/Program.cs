using ConsoleApp1;
using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.Kontur;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddVostok(SetupVostok);

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.Run();

void SetupVostok(IVostokHostingEnvironmentBuilder builder)
{
    builder.SetupApplicationIdentity(identity =>
    {
        identity.SetProject("Vostok");
        identity.SetApplication("TestAspNetCoreConsoleHosting");
        identity.SetEnvironment("dev");
    });

    builder.SetupLog(log =>
    {
        log.SetupConsoleLog();
        log.SetupFileLog(fileLog => fileLog.CustomizeSettings(
            fileLogSettings => fileLogSettings.FileOpenMode = FileOpenMode.Rewrite));
    });
    
    builder.SetupForKontur();
}