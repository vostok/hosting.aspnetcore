using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.HostBuilders;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Aspnetcore.Application;

namespace Vostok.Hosting.Aspnetcore.Builder;

[PublicAPI]
public static class WebHostBuilderExtensions
{
    public static void SetupWebHost(
        this WebApplicationBuilder webApplicationBuilder,
        IVostokHostingEnvironment environment)
    {
        var disposables = new List<IDisposable>();
        var (_, _, middlewaresBuilder, vostokWebHostBuilder) =
            VostokWebHostBuilderFactory.Create<EmptyStartup>(environment, disposables);

        middlewaresBuilder.Customize(PingApiSettingsSetup.Get(environment, webApplicationBuilder.GetType(), () => false));

        vostokWebHostBuilder.ConfigureWebHost(webApplicationBuilder);
        
        webApplicationBuilder.Services
            .AddSingleton(new DisposableContainer(disposables));
    }
}