using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Aspnetcore.Builder;

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
        var environment = VostokHostingEnvironmentFactory.Create(
            WrapSetupDelegate(setupEnvironment, webApplicationBuilder),
            settings);

        webApplicationBuilder.Logging.SetupVostok(environment);
        webApplicationBuilder.Configuration.SetupVostok(environment);
        webApplicationBuilder.SetupWebHost(environment);
        webApplicationBuilder.Services.SetupVostok(environment);
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