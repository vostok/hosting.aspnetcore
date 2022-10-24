using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Time;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore;

internal class ServiceBeaconHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IServiceBeacon serviceBeacon;
    private readonly IServer server;
    private readonly IConfiguration configuration;
    private readonly ILog log;

    public ServiceBeaconHostedService(IHostApplicationLifetime applicationLifetime, IServiceBeacon serviceBeacon, IServer server, IConfiguration configuration, ILog log)
    {
        this.applicationLifetime = applicationLifetime;
        this.serviceBeacon = serviceBeacon;
        this.server = server;
        this.configuration = configuration;
        this.log = log.ForContext<ServiceBeaconHostedService>();
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
        var urls = configuration[WebHostDefaults.ServerUrlsKey]?.Split(';');
        
        if (serviceBeacon.ReplicaInfo.TryGetUrl(out var serviceBeaconUrl))
        {
            if (!HasAddress(addresses, serviceBeaconUrl) && !HasAddress(urls, serviceBeaconUrl))
                addresses.Add($"{serviceBeaconUrl.Scheme}://*:{serviceBeaconUrl.Port}/");
            log.Info("Using url provided in Service Beacon: '{Url}'.", serviceBeaconUrl);
            return;
        }

        if (addresses.Any() || urls?.Any() == true)
        {
            throw new NotImplementedException("Dynamic configuration of Sevice Beacon is not currently supported. Please configure port explicitly during Vostok environment setup.");
        }
    }

    private static bool HasAddress(IEnumerable<string>? urls, Uri expectedUrl)
    {
        if (urls == null)
            return false;
        
        foreach (var url in urls)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var parsed) && parsed.Port == expectedUrl.Port && parsed.Scheme == expectedUrl.Scheme)
                return true;
        }

        return false;
    }
}