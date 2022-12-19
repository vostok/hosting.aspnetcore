using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Tests.TestHelpers;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests.TestHelpers
{
    [TestFixture(true)]
    public abstract partial class TestsBase
    {
        protected WebApplication WebApplication => ((TestWebApplicationHostRunner)runner).WebApplication;
        
        private void CreateRunner(VostokHostingEnvironmentSetup setup) =>
            runner = new TestWebApplicationHostRunner(setup, Setup, Setup);
        
        private void Setup(WebApplicationBuilder builder)
        {
            builder.Services.AddVostokMiddlewares(_ => {});
            //SetupGlobal(builder);
            
        }

        private void Setup(WebApplication builder)
        {
            builder.UseVostokMiddlewares();
            //SetupGlobal(builder);
        }
    }
}