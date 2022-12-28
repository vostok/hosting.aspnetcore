using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.AspNetCore.Houston.HostedServices;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Houston.External;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class UseHoustonExtensions
{
    public static WebApplicationBuilder UseHoustonHosting(this WebApplicationBuilder webApplicationBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new ExternalHoustonHost(userSetup);
        houstonHost.InitializeContext();

        webApplicationBuilder.UseVostokHosting(houstonHost.SetupEnvironment, houstonHost.Settings.ToHostingSettings());
        webApplicationBuilder.Services.UseHoustonHosting(houstonHost);

        return webApplicationBuilder;
    }

    public static IHostBuilder UseHoustonHosting(this IHostBuilder hostBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new ExternalHoustonHost(userSetup);
        houstonHost.InitializeContext();

        hostBuilder.UseVostokHosting(houstonHost.SetupEnvironment, houstonHost.Settings.ToHostingSettings());
        hostBuilder.ConfigureServices(serviceCollection => serviceCollection.UseHoustonHosting(houstonHost));

        return hostBuilder;
    }

    public static IWebHostBuilder UseHoustonHosting(this IWebHostBuilder hostBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new ExternalHoustonHost(userSetup);
        houstonHost.InitializeContext();

        hostBuilder.UseVostokHosting(houstonHost.SetupEnvironment, houstonHost.Settings.ToHostingSettings());
        hostBuilder.ConfigureServices(serviceCollection => serviceCollection.UseHoustonHosting(houstonHost));

        return hostBuilder;
    }

    private static void UseHoustonHosting(this IServiceCollection serviceCollection, ExternalHoustonHost houstonHost)
    {
        serviceCollection.ConfigureShutdownTimeout(houstonHost.ShutdownTimeout);

        serviceCollection.AddSingleton(houstonHost);

        serviceCollection.AddHostedService<HoustonHostedService>();
    }
}