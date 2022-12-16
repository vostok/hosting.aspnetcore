using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class ShutdownTimeoutTokenTests
{
    private readonly TimeSpan customShutdownTimeout = 3.3.Seconds();
    private WebApplication app;
    private WebApplicationBuilder builder;

    [SetUp]
    public void SetUp()
    {
        builder = WebApplication.CreateBuilder();

        builder.UseVostokHosting(environmentBuilder => { environmentBuilder.ApplyTestsDefaults(); });

        app = null;
    }

    [TearDown]
    public async Task TearDown()
    {
        if (app != null)
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    [Test]
    public void Should_take_shutdown_timeout_from_options()
    {
        builder.Services.Configure<HostOptions>(
            opts => opts.ShutdownTimeout = customShutdownTimeout);

        app = builder.Build();

        app.Services.GetRequiredService<IOptions<HostOptions>>()
            .Value.ShutdownTimeout.Should()
            .Be(customShutdownTimeout);
    }

    [Test]
    public void Should_not_allow_to_change_shutdown_timeout()
    {
        builder = WebApplication.CreateBuilder();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            new Action(() => { environmentBuilder.SetupShutdownTimeout(customShutdownTimeout); })
                .Should()
                .Throw<NotSupportedException>();
        });
    }

    [Test]
    public void Should_not_allow_to_use_shutdown_timeout()
    {
        app = builder.Build();

        var environment = app.Services.GetRequiredService<IVostokHostingEnvironment>();
        new Action(() => Console.WriteLine(environment.ShutdownTimeout)).Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Should_not_allow_to_change_shutdown_token()
    {
        builder = WebApplication.CreateBuilder();

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();
            new Action(() => { environmentBuilder.SetupShutdownToken(new CancellationToken()); })
                .Should()
                .Throw<NotSupportedException>();
        });
    }

    [Test]
    public void Should_not_allow_to_use_shutdown_token()
    {
        app = builder.Build();

        var environment = app.Services.GetRequiredService<IVostokHostingEnvironment>();
        new Action(() => Console.WriteLine(environment.ShutdownToken)).Should().Throw<NotSupportedException>();
    }
}