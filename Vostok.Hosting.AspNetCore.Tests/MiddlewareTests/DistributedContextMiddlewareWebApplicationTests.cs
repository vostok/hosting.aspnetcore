using NUnit.Framework;
using Vostok.Applications.AspNetCore.Tests;
using Vostok.Applications.AspNetCore.Tests.Tests;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Tests.MiddlewareTests;

[TestFixture(true)]
public class DistributedContextMiddlewareWebApplicationTests : DistributedContextMiddlewareTests  
{
    
    public DistributedContextMiddlewareWebApplicationTests(bool webApplication)
        : base(webApplication)
    {
    }

    protected override IApplicationRunner CreateRunner(VostokHostingEnvironmentSetup setup) 
        => new WebApplicationRunner(setup, SetupGlobal);

}