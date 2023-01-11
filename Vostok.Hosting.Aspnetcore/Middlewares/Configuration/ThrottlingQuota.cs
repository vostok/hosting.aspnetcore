using System;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Middlewares.Configuration;

public sealed record ThrottlingQuota(string Name, Func<PropertyQuotaOptions> OptionsProvider);