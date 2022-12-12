using ConsoleApp1;
using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.AspNetCore.Houston;
using Vostok.Hosting.AspNetCore.Houston.Applications;
using Vostok.Hosting.Houston.Abstractions;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Kontur;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

[assembly: HoustonEntryPoint(typeof(HoustonApplication))]

var builder = Host.CreateDefaultBuilder(args);

//builder.AddVostok(SetupVostok);
builder.UseHouston(SetupHouston);

builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
});

var app = builder.Build();

app.Run();

void SetupHouston(IHostingConfiguration configuration)
{
    configuration.Everywhere.SetupEnvironment(builder =>
    {
        builder.SetupApplicationIdentity(identity =>
        {
            identity.SetProject("Vostok");
            identity.SetSubproject("Test");
            identity.SetApplication("AspNetCoreHostingConsole");
            identity.SetEnvironment("dev");
        });
    });

    configuration.OutOfHouston.SetupEnvironment(builder =>
    {
        builder.SetupLog(log =>
        {
            log.SetupConsoleLog();
            log.SetupFileLog(fileLog => fileLog.CustomizeSettings(
                fileLogSettings => fileLogSettings.FileOpenMode = FileOpenMode.Rewrite));
        });
    });
}

void SetupVostok(IVostokHostingEnvironmentBuilder builder)
{
    builder.SetupApplicationIdentity(identity =>
    {
        identity.SetProject("Vostok");
        identity.SetSubproject("Test");
        identity.SetApplication("AspNetCoreHostingConsole");
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