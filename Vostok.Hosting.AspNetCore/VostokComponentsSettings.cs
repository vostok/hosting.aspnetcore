using System;
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

    // review: I think it kinda doesn't fit here (rest of the settings are cold, and this one is hot)
    //         A better solution would be to use IOptionsMonitor<TSettings> in hosted service and configure it in
    //         Conventional way using Configure(options.GetSection(...))
    // todo (kungurtsev, 14.11.2022): get ThreadPoolSettings from container?
    /// <inheritdoc cref="VostokHostSettings.ThreadPoolSettingsProvider"/>
    public Func<IConfigurationProvider, ThreadPoolSettings>? ThreadPoolSettingsProvider { get; set; }
    
    // review: IServiceProvider is already available when this list is being enumerated in hosted service.
    //         It's not rare for applications to warmup drivers for databases / client to another services.
    //         I think this code should forward service provider as an argument alongside vostok env as an extra argument
    //         or via some kind of "context" (e.g. HostBuilderContext) allowing to painlessly extend in future
    //         Or create some kind of IVostokBeforeInit and inject IEnumerable<IVostokBeforeInit>
    //         
    //         BTW as I can see IVostokHostingEnvironment is being exposed here but in other parts of code it hidden via VostokHostingEnvironmentKeeper
    /// <inheritdoc cref="VostokHostSettings.BeforeInitializeApplication"/>
    public List<Action<IVostokHostingEnvironment>> BeforeInitializeApplication { get; set; } = new();
}