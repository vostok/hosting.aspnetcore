using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class ServiceBeaconTests : TestsBase
{
    [Test]
    public async Task Should_listen_port_provided_in_beacon()
    {
        await EnsureOk();
    }
    
    [Test]
    [Ignore("Feature is not implemented")]
    public async Task Should_listen_port_provided_in_use_urls()
    {
        await EnsureOk();
    }

    protected override void SetupGlobal(WebApplication builder)
    {
        builder.MapGet("/", () => "Hello World!");
    }

    protected override void SetupGlobal(WebApplicationBuilder builder)
    {
        if (TestContext.CurrentContext.Test.Name == nameof(Should_listen_port_provided_in_use_urls))
            builder.WebHost.UseUrls($"http://localhost:{Port}");
    }

    protected override void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
    {
        if (TestContext.CurrentContext.Test.Name != nameof(Should_listen_port_provided_in_beacon))
            builder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(replica => replica.SetPort(null)));
    }

    private async Task EnsureOk()
    {
        var response = await Client.GetAsync("/");
        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");

        var beacon = WebApplication.Services.GetRequiredService<IServiceBeacon>();

        beacon.ReplicaInfo.Replica.Should().EndWith($":{Port}/");
    }
}