using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Applications.AspNetCore.Tests.Controllers;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Builder;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Tests;

public class WebApplicationRunner : IApplicationRunner
{
    private readonly WebApplicationBuilder webApplicationBuilder;
    private WebApplication webApplication;

    public WebApplicationRunner(
        VostokHostingEnvironmentSetup environmentSetup,
        VostokAspNetCoreWebApplicationSetup webApplicationSetup
    )
    {
        webApplicationBuilder = WebApplication.CreateBuilder();

        webApplicationBuilder.Services
            .AddControllers()
            .AddNewtonsoftJson()
            .AddApplicationPart(typeof(ContextController).Assembly);

        webApplicationBuilder.SetupVostok(environmentSetup);
         // webApplicationBuilder.SetupVostokWebApplication(webApplicationSetup);
    }

    public async Task RunAsync()
    {
        webApplication = webApplicationBuilder.Build();

        webApplication
            .UseRouting()
            .UseEndpoints(s => s.MapControllers())
            .UseHealthChecks("/health");
        
        var environment = (IVostokHostingEnvironment)webApplication.Services.GetService(typeof(IVostokHostingEnvironment))!;

        Task.Run(async () =>
        {
            try
            {
                await webApplication.RunAsync();
            }
            catch (Exception e)
            {
                environment.Log.Error(e);
            }
        });
        await Task.Delay(100.Milliseconds()); // ??
    }

    public Task StopAsync() =>
        webApplication.StopAsync();
}