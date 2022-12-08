using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.Object;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Tests.HostTests;

[TestFixture]
internal class ConfigurationTests : TestsBase
{
    [Test]
    public void Should_add_vostok_configuration_to_microsoft_source()
    {
        
    }
    
    protected override void SetupGlobal(WebApplicationBuilder builder)
    {
        if (TestContext.CurrentContext.Test.Name == nameof(Should_add_vostok_configuration_to_microsoft_source))
            builder.Configuration["MyOptions:B"].Should().Be("Vostok");
    }

    protected override void SetupGlobal(IVostokHostingEnvironmentBuilder builder)
    {
        builder.SetupConfiguration(configuration =>
            configuration.AddSource(new ObjectSource(new MyOptions {B = "Vostok"}).Nest("MyOptions")));
    }
    
    public class MyOptions
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
    }
}