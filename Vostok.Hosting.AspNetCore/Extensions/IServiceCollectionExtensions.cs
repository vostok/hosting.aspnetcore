﻿using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Extensions;

[PublicAPI]
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Configures <see cref="HostOptions.ShutdownTimeout"/>.
    /// </summary>
    public static void ConfigureShutdownTimeout(this IServiceCollection serviceCollection, TimeSpan timeout) =>
        serviceCollection.Configure<HostOptions>(
            opts => opts.ShutdownTimeout = timeout);

    internal static void AddVostokLoggerProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.RemoveAll<ILoggerProvider>();

        serviceCollection.Configure<VostokLoggerProviderSettings>(settings =>
            settings.IgnoredScopePrefixes = new[] {"Microsoft.AspNetCore"});

        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, VostokLoggerProvider>(serviceProvider =>
            new VostokLoggerProvider(serviceProvider.GetRequiredService<ILog>(), serviceProvider.GetFromOptionsOrDefault<VostokLoggerProviderSettings>())));
    }
}