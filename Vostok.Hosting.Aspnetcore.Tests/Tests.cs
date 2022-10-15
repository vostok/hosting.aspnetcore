using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Aspnetcore.Builder;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hosting.Aspnetcore.Tests;

[TestFixture]
internal class Tests : TestsBase
{
    [Test]
    public async Task Should_build_environment()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.SetupVostok(SetupEnvironmentDefaults);

        builder.Services.Configure<VostokHostingEnvironmentFactorySettings>(s =>
        {
            s.BeaconShutdownTimeout = 3.33.Seconds();
        });

        var app = builder.Build();

        app.MapGet("/",
            (IVostokHostingEnvironment environment) =>
            {
                environment.Log.Info("hello");
                return "Hello World!";
            });

        await Task.Run(async () =>
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