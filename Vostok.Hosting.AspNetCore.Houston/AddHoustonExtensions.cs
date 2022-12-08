using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class AddHoustonExtensions
{
    public static void AddHouston(this WebApplicationBuilder webApplicationBuilder, Action<IHostingConfiguration> userSetup) =>
        webApplicationBuilder.Services.AddHouston(userSetup, webApplicationBuilder.AddVostok);
    
    public static void AddHouston(this IHostBuilder hostBuilder, Action<IHostingConfiguration> userSetup) =>
        hostBuilder.ConfigureServices(serviceCollection => 
            serviceCollection.AddHouston(userSetup, hostBuilder.AddVostok));
    
    private static void AddHouston(this IServiceCollection serviceCollection, Action<IHostingConfiguration> userSetup, Action<VostokHostingEnvironmentSetup, VostokComponentsSettings> addVostok)
    {
        var houstonHost = new AspNetCoreHoustonHost(userSetup);
        houstonHost.ConfigureUnhandledExceptionHandling();
        
        var houstonContext = houstonHost.ObtainHoustonContextAsync().GetAwaiter().GetResult();
        houstonHost.ConfigureHostSettings(houstonContext);
        houstonHost.ConfigureHost(houstonContext);

        VostokHostingEnvironmentSetup environmentSetup = builder =>
        {
            houstonHost.EnvironmentSetup(builder);
            builder.SetupLog(logBuilder => logBuilder
                .SetupFileLog(fileLogBuilder => fileLogBuilder
                    .DisposeWithEnvironment(true)));
        };

        var hostSettings = houstonHost.Settings;
        var componentsSettings = ConvertSettings(hostSettings);

        addVostok(environmentSetup, componentsSettings);
        serviceCollection.ConfigureShutdownTimeout(houstonContext?.Setup.Shutdown.ShutdownTimeout ?? ShutdownConstants.DefaultShutdownTimeout);

        serviceCollection.AddHostedService(services =>
            new HoustonHostedService(houstonContext, services.GetRequiredService<IVostokHostingEnvironment>(), hostSettings.BeforeInitializeApplication, services.GetRequiredService<VostokApplicationStateObservable>(), services.GetRequiredService<IHostApplicationLifetime>()));
    }

    private static VostokComponentsSettings ConvertSettings(VostokHostSettings hostSettings)
    {
        var componentsSettings = new VostokComponentsSettings
        {
            ConfigureStaticProviders = hostSettings.ConfigureStaticProviders,
            ConfigureThreadPool = hostSettings.ConfigureThreadPool,
            EnvironmentWarmupSettings = new VostokHostingEnvironmentWarmupSettings
            {
                LogApplicationConfiguration = hostSettings.LogApplicationConfiguration,
                LogDotnetEnvironmentVariables = hostSettings.LogDotnetEnvironmentVariables,
                WarmupConfiguration = hostSettings.WarmupConfiguration,
                WarmupZooKeeper = hostSettings.WarmupZooKeeper
            },
            SendAnnotations = hostSettings.SendAnnotations,
            DiagnosticMetricsEnabled = hostSettings.DiagnosticMetricsEnabled,
            BeaconRegistrationWaitEnabled = hostSettings.BeaconRegistrationWaitEnabled,
            BeaconRegistrationTimeout = hostSettings.BeaconRegistrationTimeout,
            DisposeComponentTimeout = hostSettings.DisposeComponentTimeout,
            ThreadPoolTuningMultiplier = hostSettings.ThreadPoolTuningMultiplier
        };

        if (hostSettings.ThreadPoolSettingsProvider != null)
            throw new NotImplementedException("Dynamic thread pool configuration is not currently supported.");

        return componentsSettings;
    }
}