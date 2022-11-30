using System;
using Vostok.Commons.Helpers.Observable;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.AspNetCore.Helpers;

public class VostokApplicationStateObservable : IObservable<VostokApplicationState>
{
    private readonly CachingObservable<VostokApplicationState> observable = new(VostokApplicationState.NotInitialized);

    public IDisposable Subscribe(IObserver<VostokApplicationState> observer) =>
        observable.Subscribe(observer);
    
    public void ChangeStateTo(VostokApplicationState newState, Exception? error = null)
    {
        observable.Next(newState);

        if (error != null)
            observable.Error(error);
        else if (newState.IsTerminal())
            observable.Complete();
    }
}