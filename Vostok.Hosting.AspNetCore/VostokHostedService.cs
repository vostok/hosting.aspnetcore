using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Diagnostics;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ThreadPool;
using Vostok.Hosting.Helpers;

namespace Vostok.Hosting.AspNetCore;

internal class VostokHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IVostokHostingEnvironment environment;
    private readonly ILogger logger;
    private readonly VostokComponentsSettings settings;
    private DynamicThreadPoolTracker? dynamicThreadPool;

    public VostokHostedService(
        IHostApplicationLifetime applicationLifetime,
        IVostokHostingEnvironment environment,
        ILogger<VostokHostedService> logger,
        IOptions<VostokComponentsSettings> settings)
    {
        this.applicationLifetime = applicationLifetime;
        this.environment = environment;
        this.settings = settings.Value;
        this.logger = logger;
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
        logger.LogInformation("Started.");

        // todo (kungurtsev, 14.11.2022): replace/integrate with microsoft health checks?
        if (settings.DiagnosticMetricsEnabled && environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
            HealthTrackerHelper.LaunchPeriodicalChecks(diagnostics);

        if (settings.SendAnnotations)
            AnnotationsHelper.ReportInitialized(environment.ApplicationIdentity, environment.Metrics.Instance);
        
        applicationLifetime.ApplicationStopping.Register(OnStopping);
    }

    // note (kungurtsev, 14.11.2022): is called before Kestrel and Beacon stopped
    private void OnStopping()
    {
        logger.LogInformation("Stopping..");
        
        if (settings.SendAnnotations)
            AnnotationsHelper.ReportStopping(environment.ApplicationIdentity, environment.Metrics.Instance);
    }
    
    private void WarmupEnvironment()
    {
        ConfigureHostBeforeRun();

        environment.Warmup(settings.EnvironmentWarmupSettings);
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