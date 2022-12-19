using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.Object;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;

namespace Vostok.Hosting.AspNetCore.Tests.HostingTests;

[TestFixture]
internal class ConfigurationTests
{
    [Test]
    public void Should_add_vostok_configuration_to_microsoft_source()
    {
        var builder = WebApplication.CreateBuilder(new[] {"MyOptions:A=AspNetCore"});

        builder.UseVostokHosting(environmentBuilder =>
        {
            environmentBuilder.ApplyTestsDefaults();

            environmentBuilder.SetupConfiguration(configuration =>
                configuration.AddSource(new ObjectSource(new MyOptions {B = "Vostok"}).Nest("MyOptions")));
        });

        builder.Configuration["MyOptions:A"].Should().Be("AspNetCore");
        builder.Configuration["MyOptions:B"].Should().Be("Vostok");
    }

    private class MyOptions
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
    }
}