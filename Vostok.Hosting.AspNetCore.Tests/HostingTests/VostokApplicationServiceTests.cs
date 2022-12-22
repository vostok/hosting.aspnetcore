using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Testing;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Abstractions.Models;

namespace Vostok.Hosting.AspNetCore.Tests.HostingTests;

internal class VostokApplicationServiceTests
{
    private CheckingLog checkingLog;

    [TearDown]
    public void TearDown()
    {
        checkingLog?.EnsureReceivedExpectedMessages();
        checkingLog = null;
    }

    [TestCase(false)]
    [TestCase(true)]
    public void HostedServiceFromApplication_should_run_and_write_logs(bool passInstance)
    {
        checkingLog = new CheckingLog(
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[VostokApplicationHostedService`VostokWorker] Initializing application..",
            "[VostokWorker] Initialize.",
            "[VostokApplicationHostedService`VostokWorker] Application initialized.",
            "[VostokApplicationHostedService`VostokWorker] Running application.",
            // may reorder "[VostokWorker] Start.",
            "[FakeServiceBeacon] Start.",
            
            "[FakeServiceBeacon] Stop.",
            "[VostokApplicationHostedService`VostokWorker] Stopping application..",
            "[VostokWorker] Stop.",
            "[VostokApplicationHostedService`VostokWorker] Application stopped.",
            "[VostokWorker] Dispose."
        );

        var builder = CreateHostBuilder();

        builder.ConfigureServices(services =>
        {
            if (passInstance)
                services.AddHostedServiceFromApplication<VostokWorker>();
            else
                services.AddHostedServiceFromApplication(new VostokWorker());
        });

        var app = builder.Build();

        app.Start();

        Thread.Sleep(1.Seconds());

        app.StopAsync().GetAwaiter().GetResult();
        app.Dispose();
    }
    
    [TestCase(false)]
    [TestCase(true)]
    public void BackgroundServiceFromApplication_should_run_and_write_logs(bool passInstance)
    {
        checkingLog = new CheckingLog(
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[VostokApplicationBackgroundService`VostokWorker] Initializing application.",
            "[VostokWorker] Initialize.",
            "[VostokApplicationBackgroundService`VostokWorker] Running application.",
            "[VostokWorker] Start.",
            "[VostokWorker] Stop.",
            "[VostokWorker] Dispose."
        );

        var builder = CreateHostBuilder();

        builder.ConfigureServices(services =>
        {
            if (passInstance)
                services.AddBackgroundServiceFromApplication<VostokWorker>();
            else
                services.AddBackgroundServiceFromApplication(new VostokWorker());
        });

        var app = builder.Build();

        app.Start();

        Thread.Sleep(1.Seconds());

        app.StopAsync().GetAwaiter().GetResult();
        app.Dispose();
    }

    [Test]
    public void HostedServiceFromApplication_should_stop_if_initialize_stuck()
    {
        checkingLog = new CheckingLog(
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[VostokApplicationHostedService`VostokWorker] Initializing application..",
            "[VostokWorker] Initialize.",
            "[VostokApplicationHostedService`VostokWorker] Application hasn't initialized.",
            "[Microsoft.Extensions.Hosting.Internal.Host] Hosting stopped",
            "[VostokWorker] Dispose."
        );

        var builder = CreateHostBuilder();

        builder.ConfigureServices(services =>
        {
            services.AddHostedServiceFromApplication(new VostokWorker()
            {
                InfinityInitialization = true
            });
        });

        var app = builder.Build();

        Task.Run(() => app.StartAsync()).ShouldNotCompleteIn(3.Seconds());

        app.StopAsync().GetAwaiter().GetResult();
        app.Dispose();
    }
    
    [Test]
    public void HostedServiceFromApplication_should_stop_if_run_stuck()
    {
        checkingLog = new CheckingLog(
            "[VostokApplicationStateObservable] New state: Initializing.",
            "[VostokApplicationHostedService`VostokWorker] Initializing application..",
            "[VostokWorker] Initialize.",
            "[VostokApplicationHostedService`VostokWorker] Application initialized.",
            "[VostokApplicationHostedService`VostokWorker] Running application.",
            "[FakeServiceBeacon] Start.",
            
            "[FakeServiceBeacon] Stop.",
            "[VostokApplicationHostedService`VostokWorker] Stopping application..",
            "[VostokApplicationHostedService`VostokWorker] Application hasn't stopped.",
            "[VostokWorker] Dispose."
        );

        var builder = CreateHostBuilder();

        builder.ConfigureServices(services =>
        {
            services.AddHostedServiceFromApplication(new VostokWorker()
            {
                InfinityRun = true
            });

            services.ConfigureShutdownTimeout(3.Seconds());
        });

        var app = builder.Build();

        app.Start();

        Thread.Sleep(1.Seconds());

        app.StopAsync().GetAwaiter().GetResult();
        app.Dispose();
    }
    
    private IHostBuilder CreateHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            environmentBuilder.SetupLog(logBuilder => logBuilder.AddLog(checkingLog));
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IServiceBeacon>(services => new FakeServiceBeacon(
                new ReplicaInfo("default", "test", "localhost(1234)"),
                services.GetRequiredService<ILog>()));
        });
        return builder;
    }

    public class VostokWorker : IVostokApplication, IDisposable
    {
        private ILog log = null!;

        public bool InfinityInitialization { get; set; }
        public bool InfinityRun { get; set; }

        public Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            log = environment.Log.ForContext<VostokWorker>();

            log.Info("Initialize.");
            return InfinityInitialization
                ? Task.Delay(-1)
                : Task.CompletedTask;
        }

        public async Task RunAsync(IVostokHostingEnvironment environment)
        {
            log.Info("Start.");
            
            if (InfinityRun)
                await Task.Delay(-1);
            else
                await Task.Delay(-1, environment.ShutdownToken).ContinueWith(_ => {});
            
            log.Info("Stop.");
        }

        public void Dispose()
        {
            log.Info("Dispose.");
        }
    }
}