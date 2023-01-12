using System;
using Vostok.Commons.Helpers.Observable;
using Vostok.Hosting.Models;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Helpers;

public class VostokApplicationStateObservable : IObservable<VostokApplicationState>
{
    private readonly ILog log;
    private readonly CachingObservable<VostokApplicationState> observable = new(VostokApplicationState.NotInitialized);

    public VostokApplicationStateObservable(ILog log) =>
        this.log = log.ForContext<VostokApplicationStateObservable>();

    public IDisposable Subscribe(IObserver<VostokApplicationState> observer) =>
        observable.Subscribe(observer);

    public void ChangeStateTo(VostokApplicationState newState, Exception? error = null)
    {
        if (error == null)
            log.Info("New state: {State}.", newState);
        else
            log.Error(error, "Error occured. New state: {State}.", newState);

        observable.Next(newState);

        if (error != null)
            observable.Error(error);
        else if (newState.IsTerminal())
            observable.Complete();
    }
}
