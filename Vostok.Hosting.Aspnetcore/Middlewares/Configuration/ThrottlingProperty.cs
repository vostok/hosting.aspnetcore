using System;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Middlewares.Configuration;

public sealed record ThrottlingProperty(string Name, Func<HttpContext, string> ValueProvider);