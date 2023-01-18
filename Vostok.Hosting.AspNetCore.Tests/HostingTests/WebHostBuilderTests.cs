using System;
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
using Vostok.Hosting.AspNetCore.Web;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Abstractions.Models;

namespace Vostok.Hosting.AspNetCore.Tests.HostingTests;

[TestFixture(false)]
[TestFixture(true)]
internal class WebHostBuilderTests
{
    private readonly bool middlewares;
    private CheckingLog checkingLog;
    private string url;

    public WebHostBuilderTests(bool middlewares) =>
        this.middlewares = middlewares;

    [SetUp]
    public void SetUp()
    {
        url = $"http://localhost:{FreeTcpPortFinder.GetFreePort()}";
    }

    [TearDown]
    public void TearDown()
    {
        checkingLog?.EnsureReceivedExpectedMessages();
        checkingLog = null;
    }

    [Test]
    public async Task Should_start_and_stop_and_write_logs()
    {
        var toDispose = middlewares ? 4 : 0;
        checkingLog = new CheckingLog(
            "[Microsoft.AspNetCore.Hosting.Diagnostics] Hosting starting",
            "[VostokApplicationStateObservable] New state: EnvironmentWarmup.",
            "[VostokHostingEnvironment] Registered host extensions:",
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[ServiceBeaconHostedService] Using url provided in Service beacon:",
            IfMiddlewares("[Warmup] Sending request 'GET _status/ping'"),
            IfMiddlewares("[LoggingMiddleware] Received request 'GET /_status/ping' from 'Vostok.Test.SomeApplication'"),
            IfMiddlewares("[Warmup] Success. Response code = 200 ('Ok')."),
            "[FakeServiceBeacon] Start.",
            "[VostokHostedService] Started.",
            "[VostokApplicationStateObservable] New state: Running.",
            "[Microsoft.AspNetCore.Hosting.Diagnostics] Hosting shutdown",
            "[VostokHostedService] Stopping..",
            "[VostokApplicationStateObservable] New state: Stopping.",
            "[ServiceBeaconHostedService] Stopping..",
            "[FakeServiceBeacon] Stop.",
            "[VostokHostedService] Stopped.",
            "[VostokApplicationStateObservable] New state: Stopped.",
            $"[VostokHostingEnvironment] Disposing of Disposables list ({toDispose} element(s))..",
            "[VostokHostingEnvironment] Disposing of VostokHostingEnvironment..",
            "[VostokHostingEnvironment] Disposing of FileLog.."
        );

        var builder = WebHost.CreateDefaultBuilder();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            environmentBuilder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(info => info.SetEnvironment("default").SetApplication("test").SetUrl(new Uri(url))));
            environmentBuilder.SetupLog(logBuilder => logBuilder.AddLog(checkingLog));
        });

        builder.ConfigureServices(s => s.AddSingleton<IServiceBeacon>(services => new FakeServiceBeacon(
            new ReplicaInfo("default", "test", url),
            services.GetRequiredService<ILog>())));

        builder.UseStartup(_ => new Startup(middlewares));

        var app = builder.Build();

        await app.StartAsync();

        await Task.Delay(5.Seconds());

        await app.StopAsync();
        app.Dispose();
    }

    private string? IfMiddlewares(string? messageYes = null, string? messageNo = null) =>
        middlewares ? messageYes : messageNo;

    private class Startup
    {
        private readonly bool middlewares;

        public Startup(bool middlewares) =>
            this.middlewares = middlewares;

        public void ConfigureServices(IServiceCollection services)
        {
            if (middlewares)
                services.AddVostokMiddlewares();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (middlewares)
                app.UseVostokMiddlewares();

            app.Run(c => c.Response.WriteAsync("Hello world!"));
        }
    }
}