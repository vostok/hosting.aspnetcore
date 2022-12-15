using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.Houston.Configuration;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class UseHoustonExtensions
{
    public static void UseHouston(this WebApplicationBuilder webApplicationBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new AspNetCoreHoustonHost(userSetup).InitializeContext();

        webApplicationBuilder.UseVostok(houstonHost.SetupEnvironment, houstonHost.Settings);
        
        webApplicationBuilder.Services.UseHouston(houstonHost);
    }

    public static void UseHouston(this IHostBuilder hostBuilder, Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new AspNetCoreHoustonHost(userSetup).InitializeContext();

        hostBuilder.UseVostok(houstonHost.SetupEnvironment, houstonHost.Settings);
        
        hostBuilder.ConfigureServices(serviceCollection =>
            serviceCollection.UseHouston(houstonHost));
    }

    private static void UseHouston(this IServiceCollection serviceCollection, AspNetCoreHoustonHost houstonHost)
    {
        serviceCollection.ConfigureShutdownTimeout(houstonHost.ShutdownTimeout);

        serviceCollection.AddSingleton(houstonHost);
        
        serviceCollection.AddHostedService<HoustonHostedService>();
    }
}