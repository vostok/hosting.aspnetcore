using System;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

public sealed record ThrottlingQuota(string Name, Func<PropertyQuotaOptions> OptionsProvider);