using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Time;
using Vostok.Hosting.Aspnetcore.Helpers;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore;

internal class ServiceBeaconHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IServiceBeacon serviceBeacon;
    private readonly IServer server;

    public ServiceBeaconHostedService(IHostApplicationLifetime applicationLifetime, IServiceBeacon serviceBeacon, IServer server)
    {
        this.applicationLifetime = applicationLifetime;
        this.serviceBeacon = serviceBeacon;
        this.server = server;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ValidateAddresses();

        applicationLifetime.ApplicationStarted.Register(OnStartedAsync);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // todo (kungurtsev, 19.10.2022): 
    }

    private void OnStartedAsync()
    {
        serviceBeacon.Start();

        if (serviceBeacon is ServiceBeacon convertedBeacon)
        {
            convertedBeacon.WaitForInitialRegistrationAsync()
                .WaitAsync(10.Seconds())
                .ConfigureAwait(false);
        }
    }

    private void ValidateAddresses()
    {
        var addresses = server.TryGetAddresses();

        if (serviceBeacon.ReplicaInfo.TryGetUrl(out var serviceBeaconUri))
        {
            addresses.Clear();
            addresses.Add($"{serviceBeaconUri.Scheme}://{serviceBeaconUri.Host}:{serviceBeaconUri.Port}/");
        }
        // TODO else - try set serviceBeacon replicaInfo url from address.
    }
}