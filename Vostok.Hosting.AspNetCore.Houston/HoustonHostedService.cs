using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.Houston.Context;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.AspNetCore.Houston;

internal class HoustonHostedService : IHostedService
{
    private readonly HoustonContext? context;
    private readonly IVostokHostingEnvironment environment;
    private readonly List<Action<IVostokHostingEnvironment>> actions;

    public HoustonHostedService(HoustonContext? context, IVostokHostingEnvironment environment, List<Action<IVostokHostingEnvironment>> actions, VostokApplicationStateObservable applicationStateObservable)
    {
        this.context = context;
        this.environment = environment;
        this.actions = actions;

        if (context != null)
            applicationStateObservable.Subscribe(new HoustonStateObserver(context));
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var action in actions)
            action(environment);
        
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // todo (kungurtsev, 28.11.2022): pass status
        
        if (context == null)
            return;
        
        await context.Shutdown.HandleStop(new VostokApplicationRunResult(VostokApplicationState.Stopped));
    }
}