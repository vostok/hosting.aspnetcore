using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Throttling.Config;
using Vostok.Throttling.Metrics;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

[PublicAPI]
public class ThrottlingSettings
{
    private static readonly ThrottlingEssentials DefaultEssentialsInstance = new();

    public ThrottlingMetricsOptions? Metrics { get; set; } = new();
    public bool UseThreadPoolOverloadQuota { get; set; } = true;

    public int RejectionResponseCode { get; set; } = 429;
    public bool DisableForWebSockets { get; set; } = true;

    public List<ThrottlingQuota> Quotas { get; set; } = new();
    public List<ThrottlingProperty> Properties { get; set; } = new();
    public Func<ThrottlingEssentials?> Essentials { get; set; } = () => DefaultEssentialsInstance;
}