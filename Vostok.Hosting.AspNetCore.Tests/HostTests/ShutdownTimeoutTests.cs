
using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class ShutdownTimeoutTests : TestsBase
{
    private readonly TimeSpan customShutdownTimeout = 3.3.Seconds();

    [Test]
    public void Should_take_shutdown_timeout_from_options()
    {
        WebApplication.Services.GetRequiredService<IOptions<HostOptions>>()
            .Value.ShutdownTimeout.Should()
            .Be(customShutdownTimeout);
    }
    
    [Test]
    public void Should_not_allow_to_change_shutdown_timeout()
    {
    }
    
    [Test]
    public void Should_not_allow_to_use_shutdown_timeout()
    {
        var environment = WebApplication.Services.GetRequiredService<IVostokHostingEnvironment>();
        new Action(() => Console.WriteLine(environment.ShutdownTimeout)).Should().Throw<NotSupportedException>();
    }
    
    [Test]
    public void Should_not_allow_to_change_shutdown_token()
    {
    }
    
    [Test]
    public void Should_not_allow_to_use_shutdown_token()
    {
        var environment = WebApplication.Services.GetRequiredService<IVostokHostingEnvironment>();
        new Action(() => Console.WriteLine(environment.ShutdownToken)).Should().Throw<NotSupportedException>();
    }
    
    protected override void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
    {
        if (TestContext.CurrentContext.Test.Name == nameof(Should_not_allow_to_change_shutdown_timeout))
        {
            new Action(() => { builder.SetupShutdownTimeout(customShutdownTimeout); })
                .Should()
                .Throw<NotSupportedException>();
        }

        if (TestContext.CurrentContext.Test.Name == nameof(Should_not_allow_to_change_shutdown_token))
        {
            new Action(() => { builder.SetupShutdownToken(new CancellationToken());})
                .Should()
                .Throw<NotSupportedException>();
        }
    }

    protected override void SetupGlobal(WebApplicationBuilder builder)
    {
        if (TestContext.CurrentContext.Test.Name == nameof(Should_take_shutdown_timeout_from_options))
            builder.Services.Configure<HostOptions>(
                opts => opts.ShutdownTimeout = customShutdownTimeout);
    }
}