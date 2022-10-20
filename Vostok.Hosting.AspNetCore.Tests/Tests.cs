using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Tests;

[TestFixture]
internal class Tests : TestsBase
{
    protected override IApplicationRunner CreateRunner(VostokHostingEnvironmentSetup setup)
        => new MinimapApiWebApplicationRunner(setup);

    [Test]
    public async Task Should_build_environment()
    {
        var response = await Client.SendAsync(Request.Get("/"));
        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");
        await Task.Delay(5.Seconds());
    }

    private class MinimapApiWebApplicationRunner : IApplicationRunner
    {
        private readonly WebApplicationBuilder webApplicationBuilder;
        private WebApplication webApplication;

        public MinimapApiWebApplicationRunner(VostokHostingEnvironmentSetup environmentSetup)
        {
            webApplicationBuilder = WebApplication.CreateBuilder();

            webApplicationBuilder.SetupVostok(environmentSetup);

            webApplicationBuilder.Services.Configure<VostokHostingEnvironmentFactorySettings>(
                environmentFactorySettings => environmentFactorySettings.BeaconShutdownTimeout = 3.33.Seconds());
            webApplicationBuilder.Services.Configure<HostOptions>(
                hostOptions => hostOptions.ShutdownTimeout = 4.44.Seconds());
        }

        public async Task RunAsync()
        {
            webApplication = webApplicationBuilder.Build();
            webApplication.MapGet("/",
                (IVostokHostingEnvironment environment) =>
                {
                    environment.Log.Info("hello");
                    return "Hello World!";
                });

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
}