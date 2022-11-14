using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.Components.Diagnostics;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ThreadPool;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore;

internal class VostokHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IVostokHostingEnvironment environment;
    private readonly ILog log;
    private readonly VostokComponentsSettings settings;
    private DynamicThreadPoolTracker? dynamicThreadPool;

    public VostokHostedService(
        IHostApplicationLifetime applicationLifetime,
        VostokHostingEnvironmentKeeper environment,
        IOptions<VostokComponentsSettings> settings)
    {
        this.applicationLifetime = applicationLifetime;
        this.environment = environment.Environment;
        this.settings = settings.Value;
        log = this.environment.Log.ForContext<VostokHostedService>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        dynamicThreadPool = ConfigureDynamicThreadPool();
        
        WarmupEnvironment();

        applicationLifetime.ApplicationStarted.Register(OnStarted);
        
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        dynamicThreadPool?.Dispose();
        
        return Task.CompletedTask;
    }

    // note (kungurtsev, 14.11.2022): is called after Kestrel and Beacon started
    private void OnStarted()
    {
        log.Info("Started.");
        
        if (settings.DiagnosticMetricsEnabled && environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
            HealthTrackerHelper.LaunchPeriodicalChecks(diagnostics);

        if (settings.SendAnnotations)
            AnnotationsHelper.ReportInitialized(environment.ApplicationIdentity, environment.Metrics.Instance);
        
        applicationLifetime.ApplicationStopping.Register(OnStopping);
    }

    // note (kungurtsev, 14.11.2022): is called before Kestrel and Beacon stopped
    private void OnStopping()
    {
        log.Info("Stopping..");
        
        if (settings.SendAnnotations)
            AnnotationsHelper.ReportStopping(environment.ApplicationIdentity, environment.Metrics.Instance);
    }
    
    private void WarmupEnvironment()
    {
        ConfigureHostBeforeRun();

        environment.Warmup(settings.EnvironmentWarmupSettings);

        foreach (var action in settings.BeforeInitializeApplication)
            action(environment);
    }
    
    private void ConfigureHostBeforeRun()
    {
        var cpuUnitsLimit = environment.ApplicationLimits.CpuUnits;
        if (settings.ConfigureThreadPool && cpuUnitsLimit.HasValue)
            ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier, cpuUnitsLimit.Value);
    }
    
    private DynamicThreadPoolTracker? ConfigureDynamicThreadPool()
    {
        if (settings.ThreadPoolSettingsProvider == null)
            return null;
    
        var dynamicThreadPoolTracker = new DynamicThreadPoolTracker(
            () => settings.ThreadPoolSettingsProvider(environment.ConfigurationProvider),
            environment.ApplicationLimits,
            environment.Log);
    
        return dynamicThreadPoolTracker;
    }
}