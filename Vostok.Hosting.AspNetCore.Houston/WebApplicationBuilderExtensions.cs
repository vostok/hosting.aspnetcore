using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.Houston.Configuration;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class WebApplicationBuilderExtensions
{
    public static void AddHouston(
        this WebApplicationBuilder webApplicationBuilder,
        Action<IHostingConfiguration> userSetup) =>
        webApplicationBuilder.Services.AddHouston(userSetup);
    
    public static void AddHouston(
        this IHostBuilder webApplicationBuilder,
        Action<IHostingConfiguration> userSetup) =>
        webApplicationBuilder.ConfigureServices(serviceCollection => 
            serviceCollection.AddHouston(userSetup));
    
    public static void AddHouston(
        this IServiceCollection serviceCollection,
        Action<IHostingConfiguration> userSetup)
    {
        var houstonHost = new FakeHoustonHost(userSetup);
        houstonHost.ConfigureUnhandledExceptionHandling();
        
        var houstonContext = houstonHost.ObtainHoustonContextAsync().GetAwaiter().GetResult();
        houstonHost.ConfigureHostSettings(houstonContext);
        houstonHost.ConfigureHost(houstonContext);

        var environmentSetup = houstonHost.EnvironmentSetup;
        var hostSettings = houstonHost.Settings;

        Action<VostokComponentsSettings> componentsSettingsSetup = componentsSettings =>
            CopySettings(hostSettings, componentsSettings);
        
        serviceCollection.AddVostok(environmentSetup, componentsSettingsSetup);

        serviceCollection.AddHostedService(services =>
            new HoustonHostedService(houstonContext, services.GetRequiredService<IVostokHostingEnvironment>(), hostSettings.BeforeInitializeApplication, services.GetRequiredService<VostokApplicationStateObservable>()));
        
        // todo (kungurtsev, 28.11.2022): handle crashes & write postmortems
        // todo (kungurtsev, 30.11.2022): setup shutdown
    }

    private static void CopySettings(VostokHostSettings hostSettings, VostokComponentsSettings componentsSettings)
    {
        componentsSettings.ConfigureStaticProviders = hostSettings.ConfigureStaticProviders;
        componentsSettings.ConfigureThreadPool = hostSettings.ConfigureThreadPool;
        componentsSettings.EnvironmentWarmupSettings = new VostokHostingEnvironmentWarmupSettings
        {
            LogApplicationConfiguration = hostSettings.LogApplicationConfiguration,
            LogDotnetEnvironmentVariables = hostSettings.LogDotnetEnvironmentVariables,
            WarmupConfiguration = hostSettings.WarmupConfiguration,
            WarmupZooKeeper = hostSettings.WarmupZooKeeper
        };
        componentsSettings.SendAnnotations = hostSettings.SendAnnotations;
        componentsSettings.DiagnosticMetricsEnabled = hostSettings.DiagnosticMetricsEnabled;
        componentsSettings.BeaconRegistrationWaitEnabled = hostSettings.BeaconRegistrationWaitEnabled;
        componentsSettings.BeaconRegistrationTimeout = hostSettings.BeaconRegistrationTimeout;
        componentsSettings.DisposeComponentTimeout = hostSettings.DisposeComponentTimeout;
        componentsSettings.ThreadPoolTuningMultiplier = hostSettings.ThreadPoolTuningMultiplier;
        componentsSettings.ThreadPoolSettingsProvider = hostSettings.ThreadPoolSettingsProvider;
    }
}