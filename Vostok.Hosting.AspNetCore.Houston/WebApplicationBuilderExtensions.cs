using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class WebApplicationBuilderExtensions
{
    public static void AddHouston(
        this WebApplicationBuilder webApplicationBuilder,
        VostokHostingEnvironmentSetup setupEnvironment,
        Action<VostokComponentsSettings>? setupComponentsSettings = null
    )
    {
        if (setupComponentsSettings != null)
            webApplicationBuilder.Services.Configure(setupComponentsSettings);
        
        webApplicationBuilder.Services.AddSingleton(services =>
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
                setupEnvironment,
                environmentFactorySettings);

            return environment;
        });

        webApplicationBuilder.Services.AddVostokEnvironmentComponents();
        
        webApplicationBuilder.Services.AddVostokLoggerProvider();

        // review: Put my thought in "WebApplication1" :) will also try to think about possible solution
        // todo (kungurtsev, 14.11.2022): deal with configuration

        webApplicationBuilder.Services.AddHostedService<VostokHostedService>();
        webApplicationBuilder.Services.AddHostedService<ServiceBeaconHostedService>();

        webApplicationBuilder.Services.AddHealthChecks();
    }
}