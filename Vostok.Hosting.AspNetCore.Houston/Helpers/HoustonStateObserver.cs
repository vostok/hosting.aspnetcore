using System;
using Vostok.Hosting.Houston.Communication.Messages;
using Vostok.Hosting.Houston.Context;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.AspNetCore.Houston.Helpers;

internal class HoustonStateObserver : IObserver<VostokApplicationState>
{
    private readonly HoustonContext context;

    public HoustonStateObserver(HoustonContext context) =>
        this.context = context;

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(VostokApplicationState value)
    {
        context.UpdateStatus(value);
        context.Messenger.SendAsync(new InstanceStatusMessage(value));
    }
}