using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class GenericHostTests
{
    [Test]
    public void Should_run_hosted_worker()
    {
        var builder = Host.CreateDefaultBuilder();

        builder.UseVostok(SetupVostok);
        
        builder.ConfigureServices(services =>
        {
            services.AddHostedService<Worker>();
        });
        
        var app = builder.Build();

        app.Start();
        
        Thread.Sleep(5.Seconds());
        
        var hostedServices = app.Services.GetRequiredService<IEnumerable<IHostedService>>();
        var worker = hostedServices.First(s => s is Worker) as Worker;
        
        app.StopAsync().GetAwaiter().GetResult();
        app.Dispose();
        
        worker.Should().NotBeNull();
        worker!.Iteration.Should().BeGreaterThan(30);
    }

    private static void SetupVostok(IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupApplicationIdentity(identity =>
        {
            identity.SetProject("Vostok");
            identity.SetSubproject("Test");
            identity.SetApplication("AspNetCoreHostingConsole");
            identity.SetEnvironment("dev");
            identity.SetInstance("1");
        });

        builder.SetupLog(log =>
        {
            log.SetupConsoleLog();
            log.SetupFileLog(fileLog => fileLog.CustomizeSettings(
                fileLogSettings => fileLogSettings.FileOpenMode = FileOpenMode.Rewrite));
        });
    }
    
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        public long Iteration;

        public Worker(ILogger<Worker> logger) =>
            this.logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Working {Iteration}..", Iteration++);
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}