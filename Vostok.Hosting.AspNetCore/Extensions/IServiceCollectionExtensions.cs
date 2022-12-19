using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Applications.AspNetCore;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Extensions;

[PublicAPI]
public static class IServiceCollectionExtensions
{
    public static void ConfigureShutdownTimeout(this IServiceCollection serviceCollection, TimeSpan timeout) =>
        serviceCollection.Configure<HostOptions>(
            opts => opts.ShutdownTimeout = timeout);

    public static IServiceCollection AddVostokMiddlewares(this IServiceCollection services, Action<VostokMiddlewareBuilder> build)
    {
        var builder = new VostokMiddlewareBuilder();
        build(builder);

        services
            .Configure(builder.DiagnosticFeatures)
            .Configure(builder.Throttling)
            .ConfigureKestrelDefaults();

        return services
            .AddRequestTracking()
            .AddVostokHttpContextTweaks(builder.HttpContextTweaks)
            .AddVostokRequestInfo(builder.FillRequestInfo)
            .AddVostokDistributedContext(builder.DistributedContext)
            .AddVostokTracing(builder.Tracing)
            .AddThrottling(builder.ThrottlingMiddleware)
            .AddVostokRequestLogging(builder.RequestLogging)
            .AddVostokDatacenterAwareness(builder.DatacenterAwareness)
            .AddVostokUnhandledExceptions(builder.UnhandledExceptions)
            .AddVostokPingApi(builder.PingApi)
            .AddVostokDiagnosticApi(builder.DiagnosticApi);
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