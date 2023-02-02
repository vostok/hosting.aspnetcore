using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Extensions;
using Vostok.Hosting.AspNetCore.Web.Configuration;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.HostedServices;

internal class ServiceBeaconHostedService : IHostedService
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IServiceBeacon serviceBeacon;
    private readonly IServer? server;
    private readonly VostokMiddlewaresConfiguration? middlewaresConfiguration;
    private readonly IConfiguration configuration;
    private readonly IVostokHostingEnvironment environment;
    private readonly ILog log;
    private readonly VostokHostingSettings settings;
    private Uri? warmupUrl;

    public ServiceBeaconHostedService(IHostApplicationLifetime applicationLifetime, IServiceBeacon serviceBeacon, IConfiguration configuration, ILog log, IOptions<VostokHostingSettings> settings, IVostokHostingEnvironment environment, IServer? server = null, IOptions<VostokMiddlewaresConfiguration>? middlewaresConfiguration = null)
    {
        this.applicationLifetime = applicationLifetime;
        this.serviceBeacon = serviceBeacon;
        this.server = server;
        this.middlewaresConfiguration = middlewaresConfiguration?.Value;
        this.configuration = configuration;
        this.environment = environment;
        this.log = log.ForContext<ServiceBeaconHostedService>();
        this.settings = settings.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ValidateAddresses();

        applicationLifetime.ApplicationStarted.Register(OnStarted);
        applicationLifetime.ApplicationStopping.Register(OnStopping);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    // note (kungurtsev, 14.11.2022): is called after Kestrel started
    private void OnStarted()
    {
        using (new OperationContextToken("Warmup"))
        {
            if (warmupUrl != null && middlewaresConfiguration?.IsEnabled<PingApiMiddleware>() == true)
                MiddlewaresWarmup.WarmupPingApi(environment, warmupUrl).GetAwaiter().GetResult();
        }

        serviceBeacon.Start();

        WaitForServiceBeaconRegistrationIfNeeded();
    }

    // note (kungurtsev, 14.11.2022): is called before Kestrel stopped
    private void OnStopping()
    {
        log.Info("Stopping..");

        serviceBeacon.Stop();
    }

    private void WaitForServiceBeaconRegistrationIfNeeded()
    {
        if (!serviceBeacon.ReplicaInfo.TryGetUrl(out _) || !settings.BeaconRegistrationWaitEnabled || serviceBeacon is not ServiceBeacon convertedBeacon)
            return;

        if (!convertedBeacon.WaitForInitialRegistrationAsync().Wait(settings.BeaconRegistrationTimeout))
            throw new Exception($"Service beacon hasn't registered in '{settings.BeaconRegistrationTimeout}'.");
    }

    private void ValidateAddresses()
    {
        var addresses = server?.TryGetAddresses();
        var urls = configuration[WebHostDefaults.ServerUrlsKey]?.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (serviceBeacon.ReplicaInfo.TryGetUrl(out var serviceBeaconUrl))
        {
            if (addresses == null)
                throw new Exception($"Service beacon url '{serviceBeaconUrl}' is incompatible with Generic host.");

            if (!HasAddress(addresses, serviceBeaconUrl) && !HasAddress(urls, serviceBeaconUrl))
                addresses.Add($"{serviceBeaconUrl.Scheme}://*:{serviceBeaconUrl.Port}/");
            log.Info("Using url provided in Service beacon: '{Url}'.", serviceBeaconUrl);
            warmupUrl = serviceBeaconUrl;
            return;
        }

        if (serviceBeacon is not ServiceBeacon)
        {
            var url = addresses?.FirstOrDefault() ?? urls?.FirstOrDefault();
            if (url != null && TryParseUrl(url, out var parsed))
                warmupUrl = parsed;
            return;
        }

        if (addresses?.Any() == true || urls?.Any() == true)
            throw new NotImplementedException("Dynamic configuration of Service beacon is not currently supported. Please configure port explicitly during Vostok environment setup.");
    }

    private static bool HasAddress(IEnumerable<string>? urls, Uri expectedUrl)
    {
        if (urls == null)
            return false;

        foreach (var url in urls)
        {
            if (TryParseUrl(url, out var parsed) && parsed.Port == expectedUrl.Port && parsed.Scheme == expectedUrl.Scheme)
                return true;
        }

        return false;
    }

    private static bool TryParseUrl(string url, [NotNullWhen(true)] out Uri? parsed)
    {
        url = url.Replace("://*:", "://localhost:");
        return Uri.TryCreate(url, UriKind.Absolute, out parsed);
    }
}