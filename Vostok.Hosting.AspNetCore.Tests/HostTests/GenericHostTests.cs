using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Abstractions.Models;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class GenericHostTests
{
    private CheckingLog checkingLog;

    [TearDown]
    public void TearDown()
    {
        checkingLog?.EnsureReceivedExpectedMessages();
        checkingLog = null;
    }

    [Test]
    public void Should_run_hosted_worker_and_write_logs()
    {
        checkingLog = new CheckingLog(
            "[VostokApplicationStateObservable] New state: EnvironmentWarmup.",
            "[VostokHostingEnvironment] Registered host extensions:",
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[FakeServiceBeacon] Start.",
            "[VostokHostedService] Started.",
            "[VostokApplicationStateObservable] New state: Running.",
            "[Microsoft.Hosting.Lifetime] Application started.",
            "[Vostok.Hosting.AspNetCore.Tests.HostTests.GenericHostTests.Worker] Working 10..",
            "[Vostok.Hosting.AspNetCore.Tests.HostTests.GenericHostTests.Worker] Working 20..",
            "[Vostok.Hosting.AspNetCore.Tests.HostTests.GenericHostTests.Worker] Working 30..",
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

        var builder = Host.CreateDefaultBuilder();

        builder.UseVostok(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            environmentBuilder.SetupLog(logBuilder => logBuilder.AddLog(checkingLog));
        });

        builder.ConfigureServices(services =>
        {
            services.AddHostedService<Worker>();
            
            services.AddSingleton<IServiceBeacon>(services => new FakeServiceBeacon(
                new ReplicaInfo("default", "test", "localhost(1234)"),
                services.GetRequiredService<ILog>()));
        });
        
        var app = builder.Build();

        app.Start();

        Thread.Sleep(5.Seconds());

        app.StopAsync().GetAwaiter().GetResult();
        app.Dispose();
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;

        public Worker(ILogger<Worker> logger) =>
            this.logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var iteration = 0;
            
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Working {Iteration}..", iteration++);
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}