using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.AspNetCore.Application;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Builder;

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
    ) // out var environment?
    {
        var environment = VostokHostingEnvironmentFactory.Create(
            WrapSetupDelegate(setupEnvironment, webApplicationBuilder),
            settings);

        webApplicationBuilder.Logging.SetupVostok(environment);
        webApplicationBuilder.Configuration.SetupVostok(environment);
        webApplicationBuilder.Services.SetupVostok(environment);

        webApplicationBuilder.Host.ConfigureHostOptions(options => options.ShutdownTimeout = environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));
    }

    public static void SetupVostokWebApplication(
        this WebApplicationBuilder webApplicationBuilder,
        VostokAspNetCoreWebApplicationSetup setupWebApplication
    )
    {
        var environment = (IVostokHostingEnvironment) webApplicationBuilder.Services
            .First(s => s.ServiceType == typeof(IVostokHostingEnvironment))
            .ImplementationInstance!;
        
        var disposableContainer = (DisposableContainer) webApplicationBuilder.Services
            .First(s => s.ServiceType == typeof(DisposableContainer))
            .ImplementationInstance!;

        webApplicationBuilder.SetupWebApplicationBuilder(setupWebApplication, environment, out var disposables);
        disposableContainer.AddAll(disposables);
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

    private static void SetupShutdownToken(
        [NotNull] this WebApplicationBuilder webApplicationBuilder,
        IVostokHostingEnvironmentBuilder environmentBuilder
    )
    {
        var shutdownTokenSource = new CancellationTokenSource();

        environmentBuilder.SetupShutdownToken(shutdownTokenSource.Token);
        environmentBuilder.SetupShutdownTimeout(15.Seconds());
        // TODO builder.SetupShutdownTimeout(ShutdownConstants.DefaultShutdownTimeout);
        environmentBuilder.SetupHostExtensions(
            extensions =>
            {
                var vostokHostShutdown = new VostokHostShutdown(shutdownTokenSource);
                extensions.Add(vostokHostShutdown);
                extensions.Add(typeof(IVostokHostShutdown), vostokHostShutdown);
            });
        webApplicationBuilder.Services.SetupVostokShutdown(shutdownTokenSource);
    }
}