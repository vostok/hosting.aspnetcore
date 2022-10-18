using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Application;

namespace Vostok.Hosting.AspNetCore.Builder;

[PublicAPI]
public static class ServicesBuilderExtensions
{
    public static IServiceCollection SetupVostok(
        this IServiceCollection services,
        IVostokHostingEnvironment environment
    )
    {
        services
            .AddVostokEnvironment(environment)
            .AddHostedService<VostokApplicationLifeTimeService>();
        return services;
    }

    public static IServiceCollection SetupVostokShutdown(
        this IServiceCollection services,
        CancellationTokenSource shutdownTokenSource
    )
    {
        services.AddSingleton(new VostokHostShutdown(shutdownTokenSource));
        return services;
    }
}