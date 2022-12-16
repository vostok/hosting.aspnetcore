using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Abstractions.Models;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class WebApplicationTests
{
    private CheckingLog checkingLog;

    [TearDown]
    public void TearDown()
    {
        checkingLog?.EnsureReceivedExpectedMessages();
        checkingLog = null;
    }

    [Test]
    public async Task Should_start_and_stop_and_write_logs()
    {
        checkingLog = new CheckingLog(
            "[VostokApplicationStateObservable] New state: EnvironmentWarmup.",
            "[VostokHostingEnvironment] Registered host extensions:",
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[ServiceBeaconHostedService] Using url provided in Service beacon:",
            "[Microsoft.Hosting.Lifetime] Now listening on:",
            "[FakeServiceBeacon] Start.",
            "[VostokHostedService] Started.",
            "[VostokApplicationStateObservable] New state: Running.",
            "[Microsoft.Hosting.Lifetime] Application started.",
            "[VostokHostedService] Stopping..",
            "[VostokApplicationStateObservable] New state: Stopping.",
            "[ServiceBeaconHostedService] Stopping..",
            "[FakeServiceBeacon] Stop.",
            "[Microsoft.Hosting.Lifetime] Application is shutting down...",
            "[VostokHostedService] Stopped.",
            "[VostokApplicationStateObservable] New state: Stopped.",
            "[VostokHostingEnvironment] Disposing of VostokHostingEnvironment..",
            "[VostokHostingEnvironment] Disposing of FileLog.."
        );

        var builder = WebApplication.CreateBuilder();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            environmentBuilder.SetupLog(logBuilder => logBuilder.AddLog(checkingLog));
        });

        builder.Services.AddSingleton<IServiceBeacon>(services => new FakeServiceBeacon(
            new ReplicaInfo("default", "test", $"http://localhost:{FreeTcpPortFinder.GetFreePort()}"),
            services.GetRequiredService<ILog>()));

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Start();

        Thread.Sleep(5.Seconds());

        await app.StopAsync();
        await app.DisposeAsync();
    }
}