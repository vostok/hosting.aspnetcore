using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Abstractions.Models;

namespace Vostok.Hosting.AspNetCore.Tests.HostingTests;

[TestFixture]
internal class WebHostBuilderTests
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

        var builder = WebHost.CreateDefaultBuilder();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            environmentBuilder.SetupLog(logBuilder => logBuilder.AddLog(checkingLog));
        });

        builder.ConfigureServices(s => s.AddSingleton<IServiceBeacon>(services => new FakeServiceBeacon(
            new ReplicaInfo("default", "test", $"http://localhost:{FreeTcpPortFinder.GetFreePort()}"),
            services.GetRequiredService<ILog>())));

        builder.UseStartup<Startup>();

        var app = builder.Build();
        await app.StartAsync();

        await Task.Delay(5.Seconds());

        await app.StopAsync();
        app.Dispose();
    }

    private class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(c => c.Response.WriteAsync("Hello world!"));
        }
    }
}