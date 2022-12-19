using System;
using JetBrains.Annotations;
using Vostok.Hosting.Components.ThreadPool;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public class VostokHostingSettings
{
    /// <inheritdoc cref="VostokHostSettings.ConfigureStaticProviders"/>
    public bool ConfigureStaticProviders { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.ConfigureThreadPool"/>
    public bool ConfigureThreadPool { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.ThreadPoolTuningMultiplier"/>
    public int ThreadPoolTuningMultiplier { get; set; } = ThreadPoolConstants.DefaultThreadPoolMultiplier;

    /// <inheritdoc cref="VostokHostingEnvironmentWarmupSettings"/>
    public VostokHostingEnvironmentWarmupSettings EnvironmentWarmupSettings { get; set; } = new();

    /// <inheritdoc cref="VostokHostSettings.SendAnnotations"/>
    public bool SendAnnotations { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.DiagnosticMetricsEnabled"/>
    public bool DiagnosticMetricsEnabled { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.BeaconRegistrationWaitEnabled"/>
    public bool BeaconRegistrationWaitEnabled { get; set; }

    /// <inheritdoc cref="VostokHostSettings.BeaconRegistrationTimeout"/>
    public TimeSpan BeaconRegistrationTimeout { get; set; } = TimeSpan.FromSeconds(10); // collision from InternalsVisibleTo

    /// <inheritdoc cref="VostokHostSettings.DisposeComponentTimeout"/>
    public TimeSpan DisposeComponentTimeout { get; set; } = TimeSpan.FromSeconds(5);
}