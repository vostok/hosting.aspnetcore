using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Tests.TestHelpers;

public class TestWebApplicationHostRunner : ITestHostRunner
{
    public readonly WebApplication WebApplication;

    public TestWebApplicationHostRunner(VostokHostingEnvironmentSetup environmentSetup, Action<WebApplicationBuilder> webApplicationBuilderSetup, Action<WebApplication> webApplicationSetup)
    {
        var webApplicationBuilder = WebApplication.CreateBuilder();

        webApplicationBuilder.UseVostokHosting(environmentSetup);
        webApplicationBuilder.Services.ConfigureTestsDefaults();
        webApplicationBuilder.Services.AddVostokMiddlewares(_ => {});
        webApplicationBuilderSetup(webApplicationBuilder);

        WebApplication = webApplicationBuilder.Build();
        WebApplication.UseVostokMiddlewares();
        WebApplication.ConfigureTestsDefaults();
        webApplicationSetup(WebApplication);
    }

    public Task StartAsync() =>
        WebApplication.StartAsync();

    public async Task StopAsync()
    {
        await WebApplication.StopAsync();
        await WebApplication.DisposeAsync();
    }
}