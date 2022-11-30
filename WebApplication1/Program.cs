using Vostok.Hosting.AspNetCore.Houston;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Kontur;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

var builder = WebApplication.CreateBuilder(args);
// review: I'm concerned about relations between builder.Configuration and vostok environment
//         While it looks ok-ish in non-houston scenarios as developer should themself provide configuration via json files/vault/etc..
//         — most necessary configuration is available when environment is being configured.
//         But in case if CC config source is needed one to create it themself
//         
//         In houston it looks rather hash as most of the configuration is passed as part of the environment,
//         so as an application developer I have no way to configure my services using db connection strings etc.
//
//         Seems to be the only serious problem ATM. I'll think more about it later

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// review: How integration with Houston should look like at this point?
//         Overload without a delegate?
// cr (kungurtsev, 23.11.2022): probably it will be AddHouston instead of this one
//builder.AddVostok(SetupVostok);
builder.AddHouston(SetupHouston);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void SetupHouston(IHostingConfiguration configuration)
{
    configuration.Everywhere.SetupEnvironment(builder =>
    {
        builder.SetupApplicationIdentity(identity =>
        {
            identity.SetProject("Vostok");
            identity.SetSubproject("Test");
            identity.SetApplication("AspNetCoreHostingApi");
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
        
        builder.SetPort(5134);
    });
}

void SetupVostok(IVostokHostingEnvironmentBuilder builder)
{
    builder.SetupApplicationIdentity(identity =>
    {
        identity.SetProject("Vostok");
        identity.SetSubproject("Test");
        identity.SetApplication("AspNetCoreHostingApi");
        identity.SetEnvironment("dev");
    });

    builder.SetupLog(log =>
    {
        log.SetupConsoleLog();
        log.SetupFileLog(fileLog => fileLog.CustomizeSettings(
            fileLogSettings => fileLogSettings.FileOpenMode = FileOpenMode.Rewrite));
    });
    
    builder.SetPort(5134);
    
    builder.SetupForKontur();
}