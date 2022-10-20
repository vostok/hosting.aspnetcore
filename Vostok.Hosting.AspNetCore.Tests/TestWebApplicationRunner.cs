using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Tests;

public class TestWebApplicationRunner : IApplicationRunner
{
    private readonly WebApplicationBuilder webApplicationBuilder;
    private WebApplication webApplication;

    public TestWebApplicationRunner(
        VostokHostingEnvironmentSetup environmentSetup,
        Action<IVostokAspNetCoreWebApplicationBuilder, IVostokHostingEnvironment> webApplicationSetup)
    {
        webApplicationBuilder = WebApplication.CreateBuilder();
        webApplicationBuilder.Services.ConfigureServiceCollection();

        webApplicationBuilder.SetupVostok(environmentSetup);
        // webApplicationBuilder.SetupVostokWebApplication(webApplicationSetup);
    }

    public async Task RunAsync()
    {
        webApplication = webApplicationBuilder.Build();
        webApplication.ConfigureWebApplication();

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