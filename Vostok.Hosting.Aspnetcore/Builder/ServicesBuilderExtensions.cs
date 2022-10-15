using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Aspnetcore.Application;

namespace Vostok.Hosting.Aspnetcore.Builder;

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
}