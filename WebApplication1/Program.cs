using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Configuration.Sources;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Web;
using Vostok.Hosting.AspNetCore.Web.Configuration;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;
using Vostok.Throttling.Config;
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

Action<IVostokThrottlingConfigurator, IVostokHostingEnvironment> configureThrottling = (throttlingBuilder, environment) =>
{
    var configSource = environment.ConfigurationSource;
    var configProvider = environment.ConfigurationProvider;

    var essentialsSource = configSource.ScopeTo("throttling", "essentials");
    var consumerQuotaSource = configSource.ScopeTo("throttling", "consumer");
    var customQuotaSource = configSource.ScopeTo("throttling", "custom");
    throttlingBuilder.UseEssentials(() => configProvider.Get<ThrottlingEssentials>(essentialsSource));
    throttlingBuilder.UseConsumerQuota(() => configProvider.Get<PropertyQuotaOptions>(consumerQuotaSource));
    throttlingBuilder.UseCustomPropertyQuota("custom", context => context.Request.Headers["custom"], () => configProvider.Get<PropertyQuotaOptions>(customQuotaSource));
};

builder.Services
    .AddOptions<IVostokThrottlingConfigurator>()
    .Configure<IVostokHostingEnvironment>(configureThrottling);

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

app.UseRouting()
    .UseEndpoints(endpoints => endpoints.MapControllers());

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
    builder.SetupServiceBeacon(b => b.SetupReplicaInfo(i => i.SetUrlPath("hello")));
}