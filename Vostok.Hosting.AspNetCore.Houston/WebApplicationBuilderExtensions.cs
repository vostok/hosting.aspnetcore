using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Houston.Helpers;
using Vostok.Hosting.Houston.Configuration;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class WebApplicationBuilderExtensions
{
    public static void AddHouston(this WebApplicationBuilder webApplicationBuilder, Action<IHostingConfiguration> userSetup) =>
        webApplicationBuilder.AddHoustonAsync(userSetup).GetAwaiter().GetResult();
    
    public static async Task AddHoustonAsync(
        this WebApplicationBuilder webApplicationBuilder,
        Action<IHostingConfiguration> userSetup
    )
    {   
        var houstonHost = new FakeHoustonHost(userSetup);
        houstonHost.ConfigureUnhandledExceptionHandling();
        
        var houstonContext = await houstonHost.ObtainHoustonContextAsync();
        houstonHost.ConfigureHostSettings(houstonContext);
        houstonHost.ConfigureHost(houstonContext);

        var environmentSetup = houstonHost.EnvironmentSetup;
        var hostSettings = houstonHost.Settings;

        Action<VostokComponentsSettings> componentsSettingsSetup = componentsSettings =>
            CopySettings(hostSettings, componentsSettings);
        
        webApplicationBuilder.AddVostok(environmentSetup, componentsSettingsSetup);

        webApplicationBuilder.Services.AddHostedService(services =>
            new HoustonHostedService(houstonContext, services.GetRequiredService<IVostokHostingEnvironment>(), hostSettings.BeforeInitializeApplication));
        
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