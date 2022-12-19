using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.Components.Diagnostics;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Models;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.HostedServices;

internal class VostokHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IVostokHostingEnvironment environment;
    private readonly VostokApplicationStateObservable applicationStateObservable;
    private readonly VostokHostingSettings settings;
    private readonly ILog log;

    public VostokHostedService(
        IHostApplicationLifetime applicationLifetime,
        IVostokHostingEnvironment environment,
        VostokApplicationStateObservable applicationStateObservable,
        IOptions<VostokHostingSettings> settings,
        ILog log
    )
    {
        this.applicationLifetime = applicationLifetime;
        this.environment = environment;
        this.applicationStateObservable = applicationStateObservable;
        this.settings = settings.Value;
        this.log = log.ForContext<VostokHostedService>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        applicationStateObservable.ChangeStateTo(VostokApplicationState.EnvironmentWarmup);

        WarmupEnvironment();

        applicationLifetime.ApplicationStarted.Register(OnStarted);

        applicationStateObservable.ChangeStateTo(VostokApplicationState.Initializing);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    // note (kungurtsev, 14.11.2022): is called after Kestrel and Beacon started
    private void OnStarted()
    {
        log.Info("Started.");

        // todo (kungurtsev, 14.11.2022): replace/integrate with microsoft health checks?
        if (settings.DiagnosticMetricsEnabled && environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
            HealthTrackerHelper.LaunchPeriodicalChecks(diagnostics);

        if (settings.SendAnnotations)
            AnnotationsHelper.ReportInitialized(environment.ApplicationIdentity, environment.Metrics.Instance);

        applicationLifetime.ApplicationStopping.Register(OnStopping);
        applicationLifetime.ApplicationStopped.Register(OnStopped);

        applicationStateObservable.ChangeStateTo(VostokApplicationState.Running);
    }

    // note (kungurtsev, 14.11.2022): is called before Kestrel and Beacon stopped
    private void OnStopping()
    {
        log.Info("Stopping..");

        if (settings.SendAnnotations)
            AnnotationsHelper.ReportStopping(environment.ApplicationIdentity, environment.Metrics.Instance);

        applicationStateObservable.ChangeStateTo(VostokApplicationState.Stopping);
    }

    private void OnStopped()
    {
        log.Info("Stopped.");

        applicationStateObservable.ChangeStateTo(VostokApplicationState.Stopped);
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
}