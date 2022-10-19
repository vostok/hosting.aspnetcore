using System;
using System.Collections.Generic;

namespace Vostok.Hosting.AspNetCore.Application;

internal class DisposableContainer
{
    private readonly List<IDisposable> disposables;

    public DisposableContainer(List<IDisposable> disposables)
    {
        this.disposables = disposables;
    }

    public void DoDispose()
    {
        disposables.ForEach(disposable => disposable?.Dispose());
    }
}