using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.Houston.External;

namespace Vostok.Hosting.AspNetCore.Houston.HostedServices;

internal class HoustonHostedService : IHostedService
{
    private readonly IVostokHostingEnvironment environment;
    private readonly List<Action<IVostokHostingEnvironment>> actions;

    public HoustonHostedService(ExternalHoustonHost houstonHost, IVostokHostingEnvironment environment, VostokApplicationStateObservable applicationStateObservable, IHostApplicationLifetime applicationLifetime)
    {
        this.environment = environment;
        actions = houstonHost.BeforeInitializeApplication;

        houstonHost.SubscribeOnState(applicationStateObservable);
        houstonHost.RegisterOnShutdown(applicationLifetime.StopApplication);
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