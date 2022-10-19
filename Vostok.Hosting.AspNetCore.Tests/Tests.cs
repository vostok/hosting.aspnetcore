using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

#pragma warning disable CS4014

namespace Vostok.Hosting.AspNetCore.Tests;

[TestFixture]
internal class Tests : TestsBase
{
    [Test]
    public async Task Should_build_environment()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.SetupVostok(SetupEnvironmentDefaults);

        builder.Services.Configure<VostokHostingEnvironmentFactorySettings>(
            environmentFactorySettings => environmentFactorySettings.BeaconShutdownTimeout = 3.33.Seconds());
        builder.Services.Configure<HostOptions>(
            hostOptions => hostOptions.ShutdownTimeout = 4.44.Seconds());

        var app = builder.Build();

        app.MapGet("/",
            (IVostokHostingEnvironment environment) =>
            {
                environment.Log.Info("hello");
                return "Hello World!";
            });

        Task.Run(async () =>
        {
            try
            {
                //await app.RunAsync($"http://localhost:{Port}");
                await app.RunAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        });

        await Task.Delay(5.Seconds());
        var response = await Client.SendAsync(Request.Get("/"));
        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");
        await Task.Delay(5.Seconds());
    }
}