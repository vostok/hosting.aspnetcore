using System;
using System.Collections.Generic;

namespace Vostok.Hosting.AspNetCore.Application;

internal class DisposableContainer
{
    private readonly List<IDisposable> disposables = new();

    public void AddAll(List<IDisposable> list)
    {
        disposables.AddRange(list);
    }

    public void DoDispose()
    {
        disposables.ForEach(disposable => disposable?.Dispose());
    }
}