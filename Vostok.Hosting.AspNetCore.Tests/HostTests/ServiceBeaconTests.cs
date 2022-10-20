using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Applications.AspNetCore.Tests.Extensions;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class ServiceBeaconTests : TestsBase
{
    [Test]
    public async Task Should_listen_port()
    {
        await EnsureOk();
    }

    protected override void SetupGlobal(WebApplication builder)
    {
        builder.MapGet("/", () => "Hello World!");
    }

    private async Task EnsureOk()
    {
        var response = await Client.GetAsync("/");
        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");
    }
}