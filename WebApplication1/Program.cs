using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.Kontur;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddVostok(SetupVostok);

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

void SetupVostok(IVostokHostingEnvironmentBuilder builder)
{
    builder.SetupApplicationIdentity(identity =>
    {
        identity.SetProject("Vostok");
        identity.SetApplication("TestAspNetCoreHosting");
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