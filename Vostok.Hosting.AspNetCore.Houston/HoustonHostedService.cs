using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Houston.Helpers;

namespace Vostok.Hosting.AspNetCore.Houston;

internal class HoustonHostedService : IHostedService
{
    private readonly IVostokHostingEnvironment environment;
    private readonly List<Action<IVostokHostingEnvironment>> actions;

    public HoustonHostedService(AspNetCoreHoustonHost houstonHost, IVostokHostingEnvironment environment, VostokApplicationStateObservable applicationStateObservable, IHostApplicationLifetime applicationLifetime)
    {
        this.environment = environment;
        actions = houstonHost.BeforeInitializeApplication;

        if (houstonHost.Context != null)
        {
            applicationStateObservable.Subscribe(new HoustonStateObserver(houstonHost.Context));
            houstonHost.Context.Shutdown.ShutdownToken.Register(applicationLifetime.StopApplication);
        }
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var action in actions)
            action(environment);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}