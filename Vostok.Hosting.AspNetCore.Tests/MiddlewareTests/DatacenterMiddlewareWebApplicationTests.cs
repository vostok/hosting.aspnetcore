using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Applications.AspNetCore.Tests.Tests;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Tests.MiddlewareTests;

[TestFixture(true, false)]
[TestFixture(true, true)]
public class DatacenterMiddlewareWebApplicationTests : DatacenterMiddlewareTests
{
    public DatacenterMiddlewareWebApplicationTests(bool webApplication, bool rejectResponses)
        : base(webApplication, rejectResponses)
    {
    }
    
    protected override IApplicationRunner CreateRunner(VostokHostingEnvironmentSetup setup)
        => new WebApplicationRunner(setup, SetupGlobal);    
}