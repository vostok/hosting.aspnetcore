using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Vostok.Hosting.Setup;
using Vostok.Logging.File.Configuration;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class WebApplicationTests
{
    [Test]
    public async Task Should_start_and_stop()
    {
        var builder = WebApplication.CreateBuilder();

        builder.AddVostok(SetupVostok);
        
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Start();
        
        Thread.Sleep(5.Seconds());
        
        await app.StopAsync();
        await app.DisposeAsync();
    }

    private static void SetupVostok(IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupApplicationIdentity(identity =>
        {
            identity.SetProject("Vostok");
            identity.SetSubproject("Test");
            identity.SetApplication("WebApplication");
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
}