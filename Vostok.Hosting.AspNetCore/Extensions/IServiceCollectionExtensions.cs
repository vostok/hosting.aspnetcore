using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Extensions;

internal static class IServiceCollectionExtensions
{
    public static void AddVostokLoggerProvider(this IServiceCollection services)
    {
        services.RemoveAll<ILoggerProvider>();

        services.Configure<VostokLoggerProviderSettings>(settings =>
            settings.IgnoredScopePrefixes = new[] {"Microsoft.AspNetCore"});
        
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, VostokLoggerProvider>(serviceProvider =>
            new VostokLoggerProvider(serviceProvider.GetRequiredService<ILog>(), serviceProvider.GetFromOptionsOrDefault<VostokLoggerProviderSettings>())));
    }

    public static void AddOnApplicationStateChanged(this IServiceCollection services)
    {
        var onApplicationStateChanged = new VostokApplicationStateObservable();
        services.AddSingleton(onApplicationStateChanged);
    }
}