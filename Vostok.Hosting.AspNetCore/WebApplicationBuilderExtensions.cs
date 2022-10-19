using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Aspnetcore.Helpers;
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
            var factorySettings = services.GetFromOptionsOrDefault<VostokHostingEnvironmentFactorySettings>();

            return VostokHostingEnvironmentFactory.Create(
                builder =>
                {
                    SetupShutdownComponents(builder, services);
                    setupEnvironment(builder);
                },
                factorySettings);
        });

        webApplicationBuilder.Services.AddSingleton(services =>
            services.GetService<IVostokHostingEnvironment>()!.Log);

        webApplicationBuilder.Services.AddHostedService<VostokApplicationLifeTimeService>();
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