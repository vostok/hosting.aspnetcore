using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Web;
using Vostok.Hosting.AspNetCore.Web.Configuration;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;
using Vostok.Throttling.Quotas;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.UseVostokHosting(SetupVostok);

builder.Services.Configure<MyOptions>(
    builder.Configuration.GetSection("MyOptions"));

builder.Services
    .AddVostokMiddlewares()
    .ConfigureLogging(c => c.LogQueryString = new LoggingCollectionSettings(true))
    .ConfigureThrottling(t =>
    {
        t.ConfigureMiddleware(s => s.RejectionResponseCode = 503);
        t.UsePriorityQuota(() => new PropertyQuotaOptions());
    })
    .ConfigureHttpContextTweaks(c => c.EnableResponseWriteCallSizeLimit = true)
    .ConfigureDiagnosticApi(d => d.AllowOnlyLocalRequests = true)
    .ConfigureDiagnosticFeatures(f => f.AddThrottlingHealthCheck = true)
    ;

var options = builder.Configuration.GetSection("MyOptions").Get<MyOptions>();

//var root = builder.Configuration as IConfigurationRoot;
//root.GetDebugView();

var app = builder.Build();

app.UseVostokMiddlewares(); // or select only needed

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

void SetupVostok(IVostokHostingEnvironmentBuilder builder)
{
    builder.SetupApplicationIdentity(identity =>
    {
        identity.SetProject("Vostok");
        identity.SetSubproject("Test");
        identity.SetApplication("AspNetCoreHostingApi");
        identity.SetEnvironment("dev");
        identity.SetInstance("0");
    });

    builder.SetupLog(log =>
    {
        log.SetupConsoleLog();
        log.SetupFileLog(fileLog => fileLog.CustomizeSettings(
            fileLogSettings => fileLogSettings.FileOpenMode = FileOpenMode.Rewrite));
    });

    builder.SetPort(5134);
}