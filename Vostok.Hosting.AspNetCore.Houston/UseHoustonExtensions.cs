using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.Houston.Configuration;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class UseHoustonExtensions
{
    public static WebApplicationBuilder UseHoustonHosting(this WebApplicationBuilder webApplicationBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new AspNetCoreHoustonHost(userSetup).InitializeContext();

        webApplicationBuilder.UseVostokHosting(houstonHost.SetupEnvironment, houstonHost.Settings);
        webApplicationBuilder.Services.UseHoustonHosting(houstonHost);

        return webApplicationBuilder;
    }

    public static IHostBuilder UseHoustonHosting(this IHostBuilder hostBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new AspNetCoreHoustonHost(userSetup).InitializeContext();

        hostBuilder.UseVostokHosting(houstonHost.SetupEnvironment, houstonHost.Settings);
        hostBuilder.ConfigureServices(serviceCollection => serviceCollection.UseHoustonHosting(houstonHost));

        return hostBuilder;
    }

    public static IWebHostBuilder UseHoustonHosting(this IWebHostBuilder hostBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new AspNetCoreHoustonHost(userSetup).InitializeContext();

        hostBuilder.UseVostokHosting(houstonHost.SetupEnvironment, houstonHost.Settings);
        hostBuilder.ConfigureServices(serviceCollection => serviceCollection.UseHoustonHosting(houstonHost));

        return hostBuilder;
    }

    private static void UseHoustonHosting(this IServiceCollection serviceCollection, AspNetCoreHoustonHost houstonHost)
    {
        serviceCollection.ConfigureShutdownTimeout(houstonHost.ShutdownTimeout);

        serviceCollection.AddSingleton(houstonHost);

        serviceCollection.AddHostedService<HoustonHostedService>();
    }
}