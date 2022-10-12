using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery;

namespace Vostok.Hosting.Aspnetcore.Application;

internal class VostokApplicationLifeTimeService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IVostokHostingEnvironment environment;
    private readonly DisposableContainer disposableContainer;
    private readonly IVostokHostShutdown vostokHostShutdown;

    private readonly ILog log;

    public VostokApplicationLifeTimeService(
        IHostApplicationLifetime applicationLifetime,
        IVostokHostingEnvironment environment,
        DisposableContainer disposableContainer,
        IVostokHostShutdown vostokHostShutdown
    )
    {
        this.applicationLifetime = applicationLifetime;
        this.environment = environment;
        this.disposableContainer = disposableContainer;
        this.vostokHostShutdown = vostokHostShutdown;

        log = this.environment.Log.ForContext<VostokApplicationLifeTimeService>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        environment.Warmup(log);

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
        disposableContainer.DoDispose();
    }
}