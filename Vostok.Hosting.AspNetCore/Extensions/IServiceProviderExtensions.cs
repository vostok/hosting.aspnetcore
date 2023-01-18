using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Helpers;

namespace Vostok.Hosting.AspNetCore.Extensions;

internal static class IServiceProviderExtensions
{
    public static T GetFromOptionsOrThrow<T>(this IServiceProvider provider)
        where T : class, new() =>
        provider.GetFromOptionsOrNull<T>() ?? throw new Exception($"{typeof(T)} options isn't registered.");

    public static T GetFromOptionsOrDefault<T>(this IServiceProvider provider)
        where T : class, new() =>
        provider.GetFromOptionsOrNull<T>() ?? new();

    public static T? GetFromOptionsOrNull<T>(this IServiceProvider provider)
        where T : class, new() =>
        provider.GetService<IOptions<T>>()?.Value;

    public static void RegisterDisposable(this IServiceProvider serviceProvider, IDisposable disposable)
    {
        serviceProvider.GetRequiredService<VostokDisposables>()
            .Add(disposable);
    }
}