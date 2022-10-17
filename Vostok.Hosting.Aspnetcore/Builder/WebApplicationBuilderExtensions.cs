using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Aspnetcore.Application;
using Vostok.Hosting.Aspnetcore.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.Aspnetcore.Builder;

internal class VostokLoggerFactory : ILoggerFactory
{
    private readonly VostokLoggerProvider vostokLoggerProvider;

    public VostokLoggerFactory(ILog log)
    {
        vostokLoggerProvider = new VostokLoggerProvider(log);
    }

    public void Dispose()
    {
        vostokLoggerProvider.Dispose();
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return vostokLoggerProvider.CreateLogger(categoryName);
    }
}

[PublicAPI]
public static class WebApplicationBuilderExtensions
{
    public static void SetupVostok(
        [NotNull] this WebApplicationBuilder webApplicationBuilder,
        VostokHostingEnvironmentSetup setupEnvironment) =>
        SetupVostok(webApplicationBuilder, setupEnvironment, new VostokHostingEnvironmentFactorySettings());

    public static void SetupVostok(
        this WebApplicationBuilder webApplicationBuilder,
        VostokHostingEnvironmentSetup setupEnvironment,
        [NotNull] VostokHostingEnvironmentFactorySettings settings
    )
    {
        // var environment = VostokHostingEnvironmentFactory.Create(
        //     WrapSetupDelegate(setupEnvironment, webApplicationBuilder),
        //     settings);

        // webApplicationBuilder.Logging.SetupVostok(environment);
        // webApplicationBuilder.Configuration.SetupVostok(environment);
        // webApplicationBuilder.SetupWebHost(environment);
        // webApplicationBuilder.Services.SetupVostok(environment);
        
        webApplicationBuilder.Host
            .ConfigureAppConfiguration((context, configuration) => {})
            .ConfigureServices((context, collection) =>
            {
                // Will be called at WebApplicationBuilder.Build()
                collection.AddSingleton(services =>
                {
                    var factorySettings = services.GetFromOptionsOrDefault<VostokHostingEnvironmentFactorySettings>();
                    
                    return VostokHostingEnvironmentFactory.Create(
                        builder =>
                        {
                            SetupShutdownComponents(builder, services);
                            setupEnvironment(builder);
                        }, 
                        factorySettings);
                });

                collection.AddSingleton(services =>
                    services.GetService<IVostokHostingEnvironment>()!.Log);

                collection.AddHostedService<VostokApplicationLifeTimeService>();

                collection.AddSingleton<ILoggerFactory>(services =>
                {
                    var log = services.GetService<ILog>();
                    var factory = new VostokLoggerFactory(log);
                    return factory;
                });
            });
    }

    private static IVostokHostingEnvironmentBuilder SetupShutdownComponents(
        IVostokHostingEnvironmentBuilder builder,
        IServiceProvider serviceProvider)
    {
        var hostOptions = serviceProvider.GetFromOptionsOrDefault<HostOptions>();

        var shutdownTimeout = hostOptions.ShutdownTimeout;
        var shutdownTokenSource = new CancellationTokenSource();
        
        builder.SetupShutdownToken(shutdownTokenSource.Token);
        builder.SetupShutdownTimeout(shutdownTimeout);
        builder.SetupHostExtensions(
            extensions =>
            {
                var vostokHostShutdown = new VostokHostShutdown(shutdownTokenSource);
                extensions.Add(vostokHostShutdown);
                extensions.Add(typeof(IVostokHostShutdown), vostokHostShutdown);
            });

        // todo (kungurtsev, 17.10.2022): sync timeout
        // todo (kungurtsev, 17.10.2022): sync shutdown token

        return builder;
    }
}