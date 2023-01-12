using System;
using Vostok.Throttling.Quotas;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

public sealed record ThrottlingQuota(string PropertyName, Func<PropertyQuotaOptions> QuotaOptionsProvider);