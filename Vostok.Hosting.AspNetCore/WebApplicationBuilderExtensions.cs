using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public static class WebApplicationBuilderExtensions
{
    public static void SetupVostok(
        this WebApplicationBuilder webApplicationBuilder,
        VostokHostingEnvironmentSetup setupEnvironment
    )
    {
        webApplicationBuilder.Services.AddSingleton(services =>
        {
            var settings = services.GetFromOptionsOrDefault<VostokSettings>();

            var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
            {
                ConfigureStaticProviders = settings.ConfigureStaticProviders,
                // todo (kungurtsev, 24.10.2022): think about it
                BeaconShutdownWaitEnabled = false,
                DisposeComponentTimeout = settings.DisposeComponentTimeout,
                SendAnnotations = settings.SendAnnotations,
                DiagnosticMetricsEnabled = settings.DiagnosticMetricsEnabled
            };

            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);
            
            return VostokHostingEnvironmentFactory.Create(
                builder =>
                {
                    SetupShutdownComponents(builder, services);
                    setupEnvironment(builder);
                },
                environmentFactorySettings);
        });

        webApplicationBuilder.Services.AddSingleton(services =>
            services.GetService<IVostokHostingEnvironment>()!.Log);

        webApplicationBuilder.Services.AddVostokEnvironmentComponents();
        webApplicationBuilder.Services.AddVostokLoggerProvider();

        webApplicationBuilder.Services.AddHostedService<VostokHostedService>();
        webApplicationBuilder.Services.AddHostedService<ServiceBeaconHostedService>();

        webApplicationBuilder.Services.AddHealthChecks();
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