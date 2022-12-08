using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.Houston.Context;

namespace Vostok.Hosting.AspNetCore.Houston;

internal class HoustonHostedService : IHostedService
{
    private readonly IVostokHostingEnvironment environment;
    private readonly List<Action<IVostokHostingEnvironment>> actions;

    public HoustonHostedService(HoustonContext? context, IVostokHostingEnvironment environment, List<Action<IVostokHostingEnvironment>> actions, VostokApplicationStateObservable applicationStateObservable, IHostApplicationLifetime applicationLifetime)
    {
        this.environment = environment;
        this.actions = actions;

        if (context != null)
        {
            applicationStateObservable.Subscribe(new HoustonStateObserver(context));
            context.Shutdown.ShutdownToken.Register(applicationLifetime.StopApplication);
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