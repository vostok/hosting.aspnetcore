using System;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Helpers;

internal class VostokHostingEnvironmentKeeper : IDisposable
{
    public IVostokHostingEnvironment Environment { get; }

    public VostokHostingEnvironmentKeeper(IVostokHostingEnvironment environment) =>
        Environment = environment;

    public void Dispose() =>
        (Environment as IDisposable)?.Dispose();
}