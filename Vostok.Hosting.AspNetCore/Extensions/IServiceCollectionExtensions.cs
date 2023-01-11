using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Applications.AspNetCore;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.AspNetCore.Builders.Middlewares;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.MiddlewareRegistration;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Extensions;

[PublicAPI]
public static class IServiceCollectionExtensions
{
    public static void ConfigureShutdownTimeout(this IServiceCollection serviceCollection, TimeSpan timeout) =>
        serviceCollection.Configure<HostOptions>(
            opts => opts.ShutdownTimeout = timeout);

    public static IVostokMiddlewaresBuilder AddVostokMiddlewares(this IServiceCollection services)
    {
        services
            .ConfigureKestrelDefaults()
            .AddRequestTracking();

        services
            .AddVostokHttpContextTweaks(_ => {})
            .AddVostokRequestInfo(_ => {})
            .AddVostokDistributedContext(_ => {})
            .AddVostokTracing(_ => {})
            .AddThrottling()
            .AddVostokRequestLogging(_ => {})
            .AddVostokDatacenterAwareness(_ => {})
            .AddVostokUnhandledExceptions(_ => {})
            .AddVostokPingApi(_ => {})
            .AddVostokDiagnosticApi(_ => {});

        services.AddOptions<TracingSettings>()
            .Configure<IServiceBeacon>((settings, beacon) =>
            {
                if (beacon.ReplicaInfo.TryGetUrl(out var url))
                    settings.BaseUrl = url;
            });

        services.AddOptions<PingApiSettings>()
            .Configure<IVostokHostingEnvironment, InitializedFlag>((settings, environment, initFlag) =>
            {
                settings.CommitHashProvider = () => AssemblyCommitHashExtractor.ExtractFromAssembly(Assembly.GetEntryAssembly());
                settings.InitializationCheck = () => initFlag.Value;

                if (environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
                    settings.HealthCheck = () => diagnostics.HealthTracker.CurrentStatus == HealthStatus.Healthy;
            });

        return new VostokMiddlewaresBuilder(services);
    }

    internal static void AddVostokLoggerProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.RemoveAll<ILoggerProvider>();

        serviceCollection.Configure<VostokLoggerProviderSettings>(settings =>
            settings.IgnoredScopePrefixes = new[] {"Microsoft.AspNetCore"});

        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, VostokLoggerProvider>(serviceProvider =>
            new VostokLoggerProvider(serviceProvider.GetRequiredService<ILog>(), serviceProvider.GetFromOptionsOrDefault<VostokLoggerProviderSettings>())));
    }

    private static IServiceCollection ConfigureKestrelDefaults(this IServiceCollection services)
    {
        return services.Configure<KestrelServerOptions>(s =>
        {
            s.AddServerHeader = false;

            s.Limits.MaxRequestBufferSize = 128 * 1024;
            s.Limits.MaxRequestLineSize = 32 * 1024;
            s.Limits.MaxRequestHeadersTotalSize = 64 * 1024;
            s.Limits.MaxConcurrentUpgradedConnections = 10000;
        });
    }
}