using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.TestHelpers;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Tests.HostingTests;

[TestFixture]
internal class ServiceBeaconTests
{
    [Test]
    public async Task Should_listen_port_provided_in_beacon()
    {
        var builder = WebApplication.CreateBuilder();
        var port = FreeTcpPortFinder.GetFreePort();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();

            environmentBuilder.SetupServiceBeacon(beacon =>
                beacon.SetupReplicaInfo(replica => replica.SetPort(port)));
        });

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Start();

        await EnsureOk(port, app.Services.GetRequiredService<ILog>());

        await app.StopAsync();
        await app.DisposeAsync();
    }

    private static async Task EnsureOk(int port, ILog log)
    {
        var client = ClusterClientHelper.Create(port, log);

        var response = await client.GetAsync("/");
        response.Response.IsSuccessful.Should().BeTrue();
        response.Response.Content.ToString().Should().Be("Hello World!");
    }
}