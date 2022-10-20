using Vostok.Hosting.AspNetCore.Tests;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests
{
    public abstract partial class TestsBase
    {
        protected partial void InitRunner(VostokHostingEnvironmentSetup setup)
        {
            runner = CreateRunner(setup);
        }

        protected virtual IApplicationRunner CreateRunner(VostokHostingEnvironmentSetup setup)
            => new TestWebApplicationRunner(setup, SetupGlobal);
    }
}