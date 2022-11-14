using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Threading;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public static class WebApplicationBuilderExtensions
{
    public static void AddVostok(
        this WebApplicationBuilder webApplicationBuilder,
        VostokHostingEnvironmentSetup setupEnvironment
    )
    {
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

            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);
            
            var environment = VostokHostingEnvironmentFactory.Create(
                setupEnvironment,
                environmentFactorySettings);

            // note (kungurtsev, 14.11.2022): do not register IVostokHostingEnvironment
            // to prevent usage of ShutdownToken, ShutdownTimeout and Port
            return new VostokHostingEnvironmentKeeper(environment);
        });

        webApplicationBuilder.Services.AddVostokEnvironmentComponents(
            serviceProvider => serviceProvider.GetRequiredService<VostokHostingEnvironmentKeeper>().Environment);
        webApplicationBuilder.Services.AddVostokLoggerProvider();

        webApplicationBuilder.Services.AddHostedService<VostokHostedService>();
        webApplicationBuilder.Services.AddHostedService<ServiceBeaconHostedService>();

        webApplicationBuilder.Services.AddHealthChecks();
    }
}