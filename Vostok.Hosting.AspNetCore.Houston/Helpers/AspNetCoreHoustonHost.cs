using System;
using System.Collections.Generic;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Houston.Applications;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Houston;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Houston.Context;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Houston.Helpers;

internal class AspNetCoreHoustonHost : HoustonHost
{
    public HoustonContext? Context { get; private set; }

    public AspNetCoreHoustonHost(Action<IHostingConfiguration> userSetup)
        : base(new HoustonApplication(), userSetup)
    {
    }

    public AspNetCoreHoustonHost InitializeContext()
    {
        ConfigureUnhandledExceptionHandling();

        Context = ObtainHoustonContextAsync().GetAwaiter().GetResult();
        
        ConfigureHostSettings(Context);
        ConfigureHost(Context);

        return this;
    }
    
    public VostokComponentsSettings Settings =>
        ConvertSettings(settings);

    public TimeSpan ShutdownTimeout =>
        Context?.Setup.Shutdown.ShutdownTimeout ?? ShutdownConstants.DefaultShutdownTimeout;

    public List<Action<IVostokHostingEnvironment>> BeforeInitializeApplication => 
        settings.BeforeInitializeApplication;

    public void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
    {
        settings.EnvironmentSetup(builder);
        builder.SetupLog(logBuilder => logBuilder
            .SetupFileLog(fileLogBuilder => fileLogBuilder
                .DisposeWithEnvironment(true)));
    }
    
    private static VostokComponentsSettings ConvertSettings(VostokHostSettings hostSettings)
    {
        var componentsSettings = new VostokComponentsSettings
        {
            ConfigureStaticProviders = hostSettings.ConfigureStaticProviders,
            ConfigureThreadPool = hostSettings.ConfigureThreadPool,
            EnvironmentWarmupSettings = new VostokHostingEnvironmentWarmupSettings
            {
                LogApplicationConfiguration = hostSettings.LogApplicationConfiguration,
                LogDotnetEnvironmentVariables = hostSettings.LogDotnetEnvironmentVariables,
                WarmupConfiguration = hostSettings.WarmupConfiguration,
                WarmupZooKeeper = hostSettings.WarmupZooKeeper
            },
            SendAnnotations = hostSettings.SendAnnotations,
            DiagnosticMetricsEnabled = hostSettings.DiagnosticMetricsEnabled,
            BeaconRegistrationWaitEnabled = hostSettings.BeaconRegistrationWaitEnabled,
            BeaconRegistrationTimeout = hostSettings.BeaconRegistrationTimeout,
            DisposeComponentTimeout = hostSettings.DisposeComponentTimeout,
            ThreadPoolTuningMultiplier = hostSettings.ThreadPoolTuningMultiplier
        };

        if (hostSettings.ThreadPoolSettingsProvider != null)
            throw new NotImplementedException("Dynamic thread pool configuration is not currently supported.");

        return componentsSettings;
    }
}