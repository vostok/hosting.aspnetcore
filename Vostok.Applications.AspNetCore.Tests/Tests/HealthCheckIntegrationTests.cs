using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
#if NETCOREAPP
using Microsoft.Extensions.Diagnostics.HealthChecks;
#endif
using NUnit.Framework;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.Abstractions;
using HealthCheckResult = Vostok.Hosting.Abstractions.Diagnostics.HealthCheckResult;
using IHealthCheck = Vostok.Hosting.Abstractions.Diagnostics.IHealthCheck;

namespace Vostok.Applications.AspNetCore.Tests.Tests
{
    [TestFixture]
    public class HealthCheckIntegrationTests : ControllerTestBase
    {
        private volatile IVostokHostingEnvironment environment;
        private  IVostokApplicationDiagnostics diagnostics;

        protected override void SetupGlobal(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment env)
        {
            environment = env;
            environment.HostExtensions.TryGet(out diagnostics).Should().BeTrue();

            #if NETCOREAPP
            builder.SetupGenericHost(
                host => host.ConfigureServices(
                    (ctx, services) =>
                    {
                        services.AddHealthChecks().AddCheck("ms", new MicrosoftHealthCheck());
                    }));
            #endif
        }

        [Test]
        public async Task Should_include_vostok_health_checks_in_aspnetcore_middleware()
        {
            #if NETFRAMEWORK
            return;
            #endif

            await CheckHealthEndpoint(ResponseCode.Ok, "Healthy");

            using (diagnostics.HealthTracker.RegisterCheck("degraded", new DegradedHealthCheck()))
            {
                await CheckHealthEndpoint(ResponseCode.Ok, "Degraded");

                using (diagnostics.HealthTracker.RegisterCheck("failing", new FailingHealthCheck()))
                    await CheckHealthEndpoint(ResponseCode.ServiceUnavailable, "Unhealthy");

                await CheckHealthEndpoint(ResponseCode.Ok, "Degraded");
            }

            await CheckHealthEndpoint(ResponseCode.Ok, "Healthy");
        }

        [Test]
        public void Should_include_aspnetcore_health_checks_in_vostok_tracker()
        {
            #if NETFRAMEWORK
            return;
            #endif

            diagnostics.HealthTracker.Should().ContainSingle(pair => pair.name == "ms");
        }

        private async Task CheckHealthEndpoint(ResponseCode expectedCode, string expectedStatus)
        {
            var response = (await Client.GetAsync("/health")).Response;

            response.Code.Should().Be(expectedCode);
            response.Content.ToString().Should().Be(expectedStatus);
        }

        private class DegradedHealthCheck : IHealthCheck
        {
            public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken) =>
                Task.FromResult(HealthCheckResult.Degraded("Because I have degraded."));
        }

        private class FailingHealthCheck : IHealthCheck
        {
            public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken) =>
                Task.FromResult(HealthCheckResult.Failing("Because I have failed."));
        }

        #if NETCOREAPP
        private class MicrosoftHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
        {
            public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken()) =>
                Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
        }
        #endif
    }
}
