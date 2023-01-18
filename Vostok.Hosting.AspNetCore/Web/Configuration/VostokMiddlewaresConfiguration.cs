using System;
using System.Collections.Generic;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

/// <summary>
/// Represents additional configuration of Vostok middlewares.
/// </summary>
internal class VostokMiddlewaresConfiguration
{
    public readonly Dictionary<Type, bool> MiddlewareDisabled = new();
    public readonly Dictionary<Type, List<Type>> PreVostokMiddlewares = new();

    public bool IsEnabled<TMiddleware>()
    {
        if (MiddlewareDisabled.TryGetValue(typeof(TMiddleware), out var d))
            return !d;

        return true;
    }
}