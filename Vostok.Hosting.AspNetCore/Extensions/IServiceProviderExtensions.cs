﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Vostok.Hosting.AspNetCore.Extensions;

internal static class IServiceProviderExtensions
{
    public static T GetFromOptionsOrDefault<T>(this IServiceProvider provider)
        where T : class, new() =>
        provider.GetService<IOptions<T>>()?.Value ?? new();

    public static void RegisterDisposable(this IServiceProvider serviceProvider, IDisposable disposable)
    {
        serviceProvider.GetRequiredService<IHostApplicationLifetime>()
            .ApplicationStopped
            .Register(disposable.Dispose);
    }
}