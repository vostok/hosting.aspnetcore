using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
using Vostok.Hosting.Requirements;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore;

/// <summary>
/// <para>Creates <see cref="IVostokHostingEnvironment"/> instance and register all its components in DI container.</para>
/// <para>Adds Vostok hosted services that manage application and <see cref="IServiceBeacon"/> state.</para>
/// </summary>
[PublicAPI]
[SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters")]
public static class UseVostokExtensions
{
    /// <inheritdoc cref="UseVostokExtensions"/>
    public static WebApplicationBuilder UseVostokHosting(this WebApplicationBuilder webApplicationBuilder, IVostokApplication application, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
    {
        var environment = CreateEnvironment(application, environmentSetup, settings);

        webApplicationBuilder.Configuration.UseVostokHosting(environment);
        webApplicationBuilder.Services.UseVostokHosting(environment);

        return webApplicationBuilder;
    }

    /// <inheritdoc cref="UseVostokExtensions"/>
    public static IHostBuilder UseVostokHosting(this IHostBuilder hostBuilder, IVostokApplication application, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
    {
        var environment = CreateEnvironment(application, environmentSetup, settings);

        hostBuilder.ConfigureAppConfiguration(config => config.UseVostokHosting(environment));
        hostBuilder.ConfigureServices(serviceCollection => { serviceCollection.UseVostokHosting(environment); });

        return hostBuilder;
    }

    /// <inheritdoc cref="UseVostokExtensions"/>
    public static IWebHostBuilder UseVostokHosting(this IWebHostBuilder webHostBuilder, IVostokApplication application, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
    {
        var environment = CreateEnvironment(application, environmentSetup, settings);

        webHostBuilder.ConfigureAppConfiguration(config => config.UseVostokHosting(environment));
        webHostBuilder.ConfigureServices(serviceCollection => { serviceCollection.UseVostokHosting(environment); });

        return webHostBuilder;
    }

    /// <inheritdoc cref="UseVostokExtensions"/>
    public static WebApplicationBuilder UseVostokHosting(this WebApplicationBuilder webApplicationBuilder, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
        // thread: or try to locate application with ApplicationLocator?
        => webApplicationBuilder.UseVostokHosting(new EmptyVostokApplication(), environmentSetup, settings);

    /// <inheritdoc cref="UseVostokExtensions"/>
    public static IHostBuilder UseVostokHosting(this IHostBuilder hostBuilder, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
        => hostBuilder.UseVostokHosting(new EmptyVostokApplication(), environmentSetup, settings);

    /// <inheritdoc cref="UseVostokExtensions"/>
    public static IWebHostBuilder UseVostokHosting(this IWebHostBuilder webHostBuilder, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings = null)
        => webHostBuilder.UseVostokHosting(new EmptyVostokApplication(), environmentSetup, settings);

    private static void UseVostokHosting(this IConfigurationBuilder configurationBuilder, IVostokHostingEnvironment environment)
    {
        configurationBuilder.AddVostokSources(environment);
        configurationBuilder.AddDefaultLoggingFilters();
    }

    private static void UseVostokHosting(this IServiceCollection serviceCollection, IVostokHostingEnvironment environment)
    {
        serviceCollection.AddSingleton(_ => environment);
        serviceCollection.AddSingleton(new InitializedFlag());
        serviceCollection.AddSingleton<VostokDisposables>(services => new VostokDisposables(services.GetRequiredService<IVostokHostingEnvironment>().Log));

        serviceCollection.AddVostokEnvironmentComponents(environment);
        serviceCollection.AddVostokEnvironmentHostExtensions(environment);

        serviceCollection.AddVostokHealthChecks(environment);
        serviceCollection.AddSingleton<VostokApplicationStateObservable>();
        serviceCollection.AddVostokLoggerProvider();

        serviceCollection.AddHostedService<VostokHostedService>();
        serviceCollection.AddHostedService<ServiceBeaconHostedService>();

        serviceCollection.AddHealthChecks();

        serviceCollection.ConfigureShutdownTimeout(ShutdownConstants.DefaultShutdownTimeout);
    }

    private static IVostokHostingEnvironment CreateEnvironment(IVostokApplication application, VostokHostingEnvironmentSetup environmentSetup, VostokHostingSettings? settings)
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
            SetupEnvironment,
            environmentFactorySettings);

        return environment;
        
        void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
        {
            // thread: logic from VostokHost run method (not fully supported yet). Is that place correct?
            RequirementsHelper.EnsurePort(application, builder);
            RequirementsHelper.EnsureConfigurations(application, builder);

            environmentSetup(builder);
        }
    }

    private class EmptyVostokApplication : IVostokApplication
    {
        public Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            throw new Exception("Should not be called");
        }

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            throw new Exception("Should not be called");
        }
    }
}