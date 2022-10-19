using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore;

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