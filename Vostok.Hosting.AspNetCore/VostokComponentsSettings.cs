﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.ThreadPool;

namespace Vostok.Hosting.AspNetCore;

[PublicAPI]
public class VostokComponentsSettings
{
    /// <inheritdoc cref="VostokHostSettings.ConfigureStaticProviders"/>
    public bool ConfigureStaticProviders { get; set; } = true;

    /// <inheritdoc cref="VostokHostSettings.ConfigureThreadPool"/>
    public bool ConfigureThreadPool { get; set; } = true;

    // todo (kungurtsev, 14.11.2022): get VostokHostingEnvironmentWarmupSettings from container?
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

    // todo (kungurtsev, 14.11.2022): get ThreadPoolSettings from container?
    /// <inheritdoc cref="VostokHostSettings.ThreadPoolSettingsProvider"/>
    public Func<IConfigurationProvider, ThreadPoolSettings>? ThreadPoolSettingsProvider { get; set; }
    
    /// <inheritdoc cref="VostokHostSettings.BeforeInitializeApplication"/>
    public List<Action<IVostokHostingEnvironment>> BeforeInitializeApplication { get; set; } = new();
}