using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Threading;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public static class AddVostokExtensions
{
    public static void AddVostok(
        this WebApplicationBuilder webApplicationBuilder,
        VostokHostingEnvironmentSetup environmentSetup,
        Action<VostokComponentsSettings>? componentsSettingsSetup = null) =>
        webApplicationBuilder.Services.AddVostok(environmentSetup, componentsSettingsSetup);
    
    public static void AddVostok(
        this IHostBuilder hostBuilder,
        VostokHostingEnvironmentSetup environmentSetup,
        Action<VostokComponentsSettings>? componentsSettingsSetup = null) =>
        hostBuilder.ConfigureServices(serviceCollection => 
            serviceCollection.AddVostok(environmentSetup, componentsSettingsSetup));
    
    public static void AddVostok(
        this IServiceCollection serviceCollection,
        VostokHostingEnvironmentSetup environmentSetup,
        Action<VostokComponentsSettings>? componentsSettingsSetup = null)
    {
        if (componentsSettingsSetup != null)
            serviceCollection.Configure(componentsSettingsSetup);

        serviceCollection.AddSingleton(services =>
        {
            var settings = services.GetFromOptionsOrDefault<VostokComponentsSettings>();

            var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
            {
                ConfigureStaticProviders = settings.ConfigureStaticProviders,
                DisposeComponentTimeout = settings.DisposeComponentTimeout,
                SendAnnotations = settings.SendAnnotations,
                DiagnosticMetricsEnabled = settings.DiagnosticMetricsEnabled,
                SetupShutdownSupported = false
            };

            // review: Looks like this code is also included in VostokHostedService.ConfigureHostBeforeRun. Is it intentional?
            // cr (kungurtsev, 23.11.2022): yes, here we tune it with defaults without knowledge of cpu limits
            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);

            var environment = VostokHostingEnvironmentFactory.Create(
                environmentSetup,
                environmentFactorySettings);

            return environment;
        });

        serviceCollection.AddVostokEnvironmentComponents();
        serviceCollection.AddOnApplicationStateChanged();
        serviceCollection.AddVostokLoggerProvider();

        // review: Put my thought in "WebApplication1" :) will also try to think about possible solution
        // todo (kungurtsev, 14.11.2022): deal with configuration

        serviceCollection.AddHostedService<VostokHostedService>();
        serviceCollection.AddHostedService<ServiceBeaconHostedService>();

        serviceCollection.AddHealthChecks();

        // todo (kungurtsev, 28.11.2022): configure kontur static providers without BeforeInitializeApplication
    }
}