using System;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Web.Configuration;

public sealed record ThrottlingProperty(string PropertyName, Func<HttpContext, string> PropertyValueProvider);