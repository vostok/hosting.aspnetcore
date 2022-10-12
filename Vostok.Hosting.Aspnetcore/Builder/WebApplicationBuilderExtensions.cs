using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Aspnetcore.Application;
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
        
        webApplicationBuilder.Host.ConfigureServices((context, collection) =>
        {
            collection.AddSingleton(services =>
            {
                var factorySettingsOptions = services.GetService<IOptions<VostokHostingEnvironmentFactorySettings>>();
                var factorySettings = factorySettingsOptions?.Value ?? new VostokHostingEnvironmentFactorySettings();
                
                return VostokHostingEnvironmentFactory.Create(setupEnvironment, factorySettings);
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

    public static void SetupShutdownToken(
        [NotNull] this WebApplicationBuilder webApplicationBuilder,
        IVostokHostingEnvironmentBuilder environmentBuilder
    )
    {
        var shutdownTokenSource = new CancellationTokenSource();

        environmentBuilder.SetupShutdownToken(shutdownTokenSource.Token);
        environmentBuilder.SetupShutdownTimeout(15.Seconds());
        // builder.SetupShutdownTimeout(ShutdownConstants.DefaultShutdownTimeout);
        environmentBuilder.SetupHostExtensions(
            extensions =>
            {
                var vostokHostShutdown = new VostokHostShutdown(shutdownTokenSource);
                extensions.Add(vostokHostShutdown);
                extensions.Add(typeof(IVostokHostShutdown), vostokHostShutdown);
            });
        webApplicationBuilder.Services.SetupVostokShutdown(shutdownTokenSource);
    }

    private static VostokHostingEnvironmentSetup WrapSetupDelegate(
        VostokHostingEnvironmentSetup setup,
        WebApplicationBuilder webApplicationBuilder)
    {
        return builder =>
        {
            webApplicationBuilder.SetupShutdownToken(builder);
            setup(builder);
        };
    }
}