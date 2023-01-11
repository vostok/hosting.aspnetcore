using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Abstractions.Models;

namespace Vostok.Hosting.AspNetCore.Tests.HostingTests;

[TestFixture(false)]
[TestFixture(true)]
internal class WebApplicationTests
{
    private readonly bool middlewares;
    private CheckingLog checkingLog;
    private string url;

    public WebApplicationTests(bool middlewares) =>
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
            "[VostokApplicationStateObservable] New state: EnvironmentWarmup.",
            "[VostokHostingEnvironment] Registered host extensions:",
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[ServiceBeaconHostedService] Using url provided in Service beacon:",
            "[Microsoft.Hosting.Lifetime] Now listening on:",
            IfMiddlewares("[Warmup] Sending request 'GET _status/ping'"),
            IfMiddlewares("[LoggingMiddleware] Received request 'GET /_status/ping' from 'Vostok.Test.SomeApplication'"),
            IfMiddlewares("[Warmup] Success. Response code = 200 ('Ok')."),
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
            $"[VostokHostingEnvironment] Disposing of Disposables list ({toDispose} element(s))..",
            "[VostokHostingEnvironment] Disposing of VostokHostingEnvironment..",
            "[VostokHostingEnvironment] Disposing of FileLog.."
        );

        var builder = WebApplication.CreateBuilder();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            environmentBuilder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(info => info.SetEnvironment("default").SetApplication("test").SetUrl(new Uri(url))));
            environmentBuilder.SetupLog(logBuilder => logBuilder.AddLog(checkingLog));
        });

        builder.Services.AddSingleton<IServiceBeacon>(services => new FakeServiceBeacon(
            new ReplicaInfo("default", "test", url),
            services.GetRequiredService<ILog>()));

        if (middlewares)
            builder.Services.AddVostokMiddlewares();

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        if (middlewares)
            app.UseVostokMiddlewares();
        
        app.Start();

        Thread.Sleep(5.Seconds());

        await app.StopAsync();
        await app.DisposeAsync();
    }

    private string? IfMiddlewares(string? messageYes = null, string? messageNo = null) =>
        middlewares ? messageYes : messageNo;
}