using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.HostedServices;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public static class UseVostokExtensions
{
    public static WebApplicationBuilder UseVostokHosting(this WebApplicationBuilder webApplicationBuilder, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
    {
        var environment = CreateEnvironment(environmentSetup, settings);

        webApplicationBuilder.Configuration.UseVostokHosting(environment);
        webApplicationBuilder.Services.UseVostokHosting(environment);

        return webApplicationBuilder;
    }

    public static IHostBuilder UseVostokHosting(this IHostBuilder hostBuilder, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
    {
        var environment = CreateEnvironment(environmentSetup, settings);

        hostBuilder.ConfigureAppConfiguration(config => config.UseVostokHosting(environment));
        hostBuilder.ConfigureServices(serviceCollection => { serviceCollection.UseVostokHosting(environment); });

        return hostBuilder;
    }

    public static IWebHostBuilder UseVostokHosting(this IWebHostBuilder hostBuilder, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
    {
        var environment = CreateEnvironment(environmentSetup, settings);

        hostBuilder.ConfigureAppConfiguration(config => config.UseVostokHosting(environment));
        hostBuilder.ConfigureServices(serviceCollection => { serviceCollection.UseVostokHosting(environment); });

        return hostBuilder;
    }

    private static void UseVostokHosting(this IConfigurationBuilder configurationBuilder, IVostokHostingEnvironment environment)
    {
        configurationBuilder.AddVostokSources(environment);
        configurationBuilder.AddDefaultLoggingFilters();
    }

    private static void UseVostokHosting(this IServiceCollection serviceCollection, IVostokHostingEnvironment environment)
    {
        serviceCollection.AddSingleton(_ => environment);

        serviceCollection.AddVostokEnvironmentComponents();
        serviceCollection.AddVostokEnvironmentHostExtensions(environment);
        serviceCollection.AddSingleton<VostokApplicationStateObservable>();
        serviceCollection.AddVostokLoggerProvider();

        serviceCollection.AddHostedService<VostokHostedService>();
        serviceCollection.AddHostedService<ServiceBeaconHostedService>();

        serviceCollection.AddHealthChecks();

        serviceCollection.ConfigureShutdownTimeout(ShutdownConstants.DefaultShutdownTimeout);
    }

    private static IVostokHostingEnvironment CreateEnvironment(VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings)
    {
        settings ??= new VostokHostingSettings();

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