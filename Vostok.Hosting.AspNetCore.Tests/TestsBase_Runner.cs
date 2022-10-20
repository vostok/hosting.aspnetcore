using NUnit.Framework;
using Vostok.Hosting.AspNetCore.Tests;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests
{
    [TestFixture(true)]
    public abstract partial class TestsBase
    {
        private void CreateRunner(VostokHostingEnvironmentSetup setup) =>
            runner = new TestWebApplicationHostRunner(setup, SetupGlobal, SetupGlobal);
    }
}