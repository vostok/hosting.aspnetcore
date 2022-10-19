using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Applications.AspNetCore.Tests.Tests;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Tests.MiddlewareTests;

[TestFixture(true)]
public class PingApiMiddlewareWebApplicationTests : PingApiMiddlewareTests
{
    public PingApiMiddlewareWebApplicationTests(bool webApplication)
        : base(webApplication)
    {
    }
    
    protected override IApplicationRunner CreateRunner(VostokHostingEnvironmentSetup setup)
        => new TestWebApplicationRunner(setup, SetupGlobal);    
}