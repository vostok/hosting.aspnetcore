﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Vostok.Hosting.Aspnetcore.Helpers;

internal static class IServiceProviderExtensions
{
    public static T GetFromOptionsOrDefault<T>(this IServiceProvider provider)
        where T : class, new() =>
        provider.GetService<IOptions<T>>()?.Value ?? new();
}