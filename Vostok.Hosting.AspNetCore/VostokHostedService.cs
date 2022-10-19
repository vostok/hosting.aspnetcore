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
using Vostok.Hosting.Aspnetcore.Helpers;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore;

internal class ServiceBeaconHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IServiceBeacon serviceBeacon;

    public ServiceBeaconHostedService(IHostApplicationLifetime applicationLifetime, IServiceBeacon serviceBeacon)
    {
        this.applicationLifetime = applicationLifetime;
        this.serviceBeacon = serviceBeacon;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!await applicationLifetime.TryWaitStartedAsync(cancellationToken))
            return;
        
        serviceBeacon.Start();

        if (serviceBeacon is ServiceBeacon convertedBeacon)
        {
            // convertedBeacon.WaitForInitialRegistrationAsync()
            //     .WaitAsync(10.Seconds())
            //     .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        
    }
}

internal class VostokHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IVostokHostingEnvironment environment;
    private readonly IServiceProvider serviceProvider;
    // private readonly DisposableContainer disposableContainer;
    // private readonly IVostokHostShutdown vostokHostShutdown;

    private readonly ILog log;

    public VostokHostedService(
        IHostApplicationLifetime applicationLifetime,
        IVostokHostingEnvironment environment,
        IServiceProvider serviceProvider
        //DisposableContainer disposableContainer,
        //IVostokHostShutdown vostokHostShutdown
    )
    {
        this.applicationLifetime = applicationLifetime;
        this.environment = environment;
        this.serviceProvider = serviceProvider;
        //this.vostokHostShutdown = vostokHostShutdown;
        // this.disposableContainer = disposableContainer;

        log = this.environment.Log.ForContext<VostokHostedService>();

        var server = serviceProvider.GetRequiredService<IServer>();
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

    private void OnStartedAsync()
    {
        log.Info("OnStarted application life time cycle event");

        var server = serviceProvider.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();
        var addresses = addressFeature.Addresses.ToList();

        
    }

    private void OnStopping()
    {
        log.Info("OnStopping application life time cycle event");

        //vostokHostShutdown?.Initiate();
    }

    private void OnStopped()
    {
        log.Info("OnStopped application life time cycle event");

        (environment as IDisposable)?.Dispose();
        // disposableContainer.DoDispose();
    }
}