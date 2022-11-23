using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Threading;
using Vostok.Hosting.AspNetCore.Extensions;
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
            // review: While it's possible to configure VostokComponentsSettings using .Configure<T>(...) call
            //         current method's api doesn't lead developer doing so as knowledge of existing of such type can only be
            //         obtained from the source code. I think it would be useful to include it in a typical way
            //         (via optional Action<T> and calling the Configure<T>(action) internally)
            //
            //         It's typical for lots of MS apis and other libraries — check almost any Add* method from ASP.NET Core
            //         it most likely will have an overload of any method accepting Action<T> for configuration as it leads
            //         to library api being easily explorable. 
            //         Another example is returning some sort of builder like IMvcBuilder (AddControllers)
            var settings = services.GetFromOptionsOrDefault<VostokComponentsSettings>();

            var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
            {
                ConfigureStaticProviders = settings.ConfigureStaticProviders,
                DisposeComponentTimeout = settings.DisposeComponentTimeout,
                SendAnnotations = settings.SendAnnotations,
                DiagnosticMetricsEnabled = settings.DiagnosticMetricsEnabled,
                SetupShutdownSupported = false
            };

            // review: Looks like this code is also included in VostokHostedService.ConfigureHostBeforeRun. Is it intentional?
            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);
            
            var environment = VostokHostingEnvironmentFactory.Create(
                setupEnvironment,
                environmentFactorySettings);

            return environment;
        });

        webApplicationBuilder.Services.AddVostokEnvironmentComponents();
        
        webApplicationBuilder.Services.AddVostokLoggerProvider();

        // review: Put my thought in "WebApplication1" :) will also try to think about possible solution
        // todo (kungurtsev, 14.11.2022): deal with configuration

        webApplicationBuilder.Services.AddHostedService<VostokHostedService>();
        webApplicationBuilder.Services.AddHostedService<ServiceBeaconHostedService>();

        webApplicationBuilder.Services.AddHealthChecks();
    }
}