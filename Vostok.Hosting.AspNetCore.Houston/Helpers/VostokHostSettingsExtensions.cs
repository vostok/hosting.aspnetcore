using System;

namespace Vostok.Hosting.AspNetCore.Houston.Helpers;

internal static class VostokHostSettingsExtensions
{
    public static VostokHostingSettings ToHostingSettings(this VostokHostSettings hostSettings)
    {
        var hostingSettings = new VostokHostingSettings
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

        return hostingSettings;
    }
}