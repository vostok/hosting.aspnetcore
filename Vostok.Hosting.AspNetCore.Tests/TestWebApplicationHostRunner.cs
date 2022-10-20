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

public class TestWebApplicationHostRunner : ITestHostRunner
{
    private readonly WebApplication webApplication;

    public TestWebApplicationHostRunner(VostokHostingEnvironmentSetup environmentSetup, Action<WebApplicationBuilder> webApplicationBuilderSetup, Action<WebApplication> webApplicationSetup)
    {
        var webApplicationBuilder = WebApplication.CreateBuilder();
        
        webApplicationBuilder.SetupVostok(environmentSetup);
        webApplicationBuilder.Services.ConfigureTestsDefaults();
        webApplicationBuilderSetup(webApplicationBuilder);
        
        webApplication = webApplicationBuilder.Build();
        
        webApplication.ConfigureTestsDefaults();
        webApplicationSetup(webApplication);
    }

    public Task StartAsync() =>
        webApplication.StartAsync();

    public Task StopAsync() =>
        webApplication.StopAsync();
}