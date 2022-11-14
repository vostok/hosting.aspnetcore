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
    public readonly WebApplication WebApplication;

    public TestWebApplicationHostRunner(VostokHostingEnvironmentSetup environmentSetup, Action<WebApplicationBuilder> webApplicationBuilderSetup, Action<WebApplication> webApplicationSetup)
    {
        var webApplicationBuilder = WebApplication.CreateBuilder();
        
        webApplicationBuilder.AddVostok(environmentSetup);
        webApplicationBuilder.Services.ConfigureTestsDefaults();
        webApplicationBuilderSetup(webApplicationBuilder);
        
        WebApplication = webApplicationBuilder.Build();
        
        WebApplication.ConfigureTestsDefaults();
        webApplicationSetup(WebApplication);
    }

    public Task StartAsync() =>
        WebApplication.StartAsync();

    public Task StopAsync() =>
        WebApplication.StopAsync();
}