using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public static class AddVostokExtensions
{
    public static void AddVostok(
        this WebApplicationBuilder webApplicationBuilder,
        VostokHostingEnvironmentSetup environmentSetup,
        VostokComponentsSettings? settings = null)
    {
        var environment = CreateEnvironment(environmentSetup, settings);

        webApplicationBuilder.Configuration.AddVostokSources(environment);
        
        webApplicationBuilder.Services.AddVostok(environment);
    }

    public static void AddVostok(
        this IHostBuilder hostBuilder,
        VostokHostingEnvironmentSetup environmentSetup,
        VostokComponentsSettings? settings = null)
    {
        var environment = CreateEnvironment(environmentSetup, settings);

        hostBuilder.ConfigureAppConfiguration(config => config.AddVostokSources(environment));

        hostBuilder.ConfigureServices(serviceCollection =>
        {
            serviceCollection.AddVostok(environment);
        });
    }

    private static void AddVostok(this IServiceCollection serviceCollection, IVostokHostingEnvironment environment)
    {
        serviceCollection.AddSingleton(environment);

        serviceCollection.AddVostokEnvironmentComponents();
        serviceCollection.AddOnApplicationStateChanged();
        serviceCollection.AddVostokLoggerProvider();

        serviceCollection.AddHostedService<VostokHostedService>();
        serviceCollection.AddHostedService<ServiceBeaconHostedService>();

        serviceCollection.AddHealthChecks();

        serviceCollection.ConfigureShutdownTimeout(ShutdownConstants.DefaultShutdownTimeout);
    }

    private static IVostokHostingEnvironment CreateEnvironment(VostokHostingEnvironmentSetup environmentSetup, VostokComponentsSettings? settings)
    {
        settings ??= new VostokComponentsSettings();

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
            environmentSetup,
            environmentFactorySettings);
        
        return environment;
    }
}