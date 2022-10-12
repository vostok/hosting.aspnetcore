using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Aspnetcore.Builder;

namespace Vostok.Hosting.Aspnetcore.Tests;

[TestFixture]
internal class Tests : TestsBase
{
    [Test]
    public async Task Should_build_environment()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.SetupVostok(SetupEnvironmentDefaults);
        
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        Task.Run(() => app.RunAsync($"http://localhost:{Port}"));
        
        var response = await Client.SendAsync(Request.Get("/"));
        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");
    }
}