using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Application;

namespace Vostok.Hosting.AspNetCore.Builder;

[PublicAPI]
public static class WebHostBuilderExtensions
{
    public static void SetupWebHost(
        this WebApplicationBuilder webApplicationBuilder,
        IVostokHostingEnvironment environment)
    {
        var disposables = new List<IDisposable>();
        webApplicationBuilder.Services
            .AddSingleton(new DisposableContainer(disposables));
    }
}