using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Components.ThreadPool;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public class VostokComponentsSettings
{
    /// <inheritdoc cref="VostokHostSettings.ConfigureStaticProviders"/>
    public bool ConfigureStaticProviders { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.ConfigureThreadPool"/>
    public bool ConfigureThreadPool { get; set; } = true;

    /// <inheritdoc cref="VostokHostingEnvironmentWarmupSettings"/>
    public VostokHostingEnvironmentWarmupSettings EnvironmentWarmupSettings { get; set; } = new();
    
    /// <inheritdoc cref="VostokHostSettings.SendAnnotations"/>
    public bool SendAnnotations { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.DiagnosticMetricsEnabled"/>
    public bool DiagnosticMetricsEnabled { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.BeaconRegistrationWaitEnabled"/>
    public bool BeaconRegistrationWaitEnabled { get; set; }

    /// <inheritdoc cref="VostokHostSettings.BeaconRegistrationTimeout"/>
    public TimeSpan BeaconRegistrationTimeout { get; set; } = 10.Seconds();

    /// <inheritdoc cref="VostokHostSettings.DisposeComponentTimeout"/>
    public TimeSpan DisposeComponentTimeout { get; set; } = 5.Seconds();

    /// <inheritdoc cref="VostokHostSettings.ThreadPoolTuningMultiplier"/>
    public int ThreadPoolTuningMultiplier { get; set; } = ThreadPoolConstants.DefaultThreadPoolMultiplier;
}