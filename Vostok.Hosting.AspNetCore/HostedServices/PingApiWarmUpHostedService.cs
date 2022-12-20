using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.HostedServices;

internal class PingApiWarmUpHostedService : IHostedService
{
    private readonly IVostokHostingEnvironment environment;

    public PingApiWarmUpHostedService(IVostokHostingEnvironment environment)
    {
        this.environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await MiddlewaresWarmup.WarmupPingApi(environment);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}