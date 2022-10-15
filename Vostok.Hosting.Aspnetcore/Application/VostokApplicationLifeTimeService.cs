using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Aspnetcore.Application;

internal class VostokApplicationLifeTimeService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IVostokHostingEnvironment environment;
    private readonly IServiceProvider services;
    // private readonly DisposableContainer disposableContainer;
    private readonly IVostokHostShutdown vostokHostShutdown;

    private readonly ILog log;

    public VostokApplicationLifeTimeService(
        IHostApplicationLifetime applicationLifetime,
        IVostokHostingEnvironment environment,
        IServiceProvider services,
        //DisposableContainer disposableContainer,
        IVostokHostShutdown vostokHostShutdown
    )
    {
        this.applicationLifetime = applicationLifetime;
        this.environment = environment;
        this.services = services;
        this.vostokHostShutdown = vostokHostShutdown;
        // this.disposableContainer = disposableContainer;

        log = this.environment.Log.ForContext<VostokApplicationLifeTimeService>();

        var server = services.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();

        if (environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var serviceBeaconUri))
        {
            var address = addressFeature.Addresses.FirstOrDefault(address =>
            {
                if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    return uri.Scheme != serviceBeaconUri.Scheme || uri.Port != serviceBeaconUri.Port;
                }

                return false;
            });
            if (address != null)
            {
                throw new ArgumentException($"Duplicated configuration for port or url." +
                                            $"ServiceBeacon url: {serviceBeaconUri}, application url: {address}");
            }
            
            addressFeature.Addresses.Add($"{serviceBeaconUri.Scheme}://{serviceBeaconUri.Host}:{serviceBeaconUri.Port}/");
            // addressFeature.Addresses.Add(serviceBeaconUri.ToString());
        }
        // TODO else - try set serviceBeacon replicaInfo url from address.
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        environment.Warmup(new VostokHostingEnvironmentWarmupSettings());

        applicationLifetime.ApplicationStarted.Register(OnStartedAsync);
        applicationLifetime.ApplicationStopping.Register(OnStopping);
        applicationLifetime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async void OnStartedAsync()
    {
        log.Info("OnStarted application life time cycle event");

        var server = services.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();
        var addresses = addressFeature.Addresses.ToList();

        environment.ServiceBeacon.Start();

        if (environment.ServiceBeacon is ServiceBeacon convertedBeacon)
        {
            await convertedBeacon.WaitForInitialRegistrationAsync()
                .WaitAsync(10.Seconds())
                .ConfigureAwait(false);
        }
    }

    private void OnStopping()
    {
        log.Info("OnStopping application life time cycle event");

        vostokHostShutdown?.Initiate();
    }

    private void OnStopped()
    {
        log.Info("OnStopped application life time cycle event");

        (environment as IDisposable)?.Dispose();
        // disposableContainer.DoDispose();
    }
}