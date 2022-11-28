using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Hosting.Houston.Configuration;

namespace Vostok.Hosting.AspNetCore.Houston;

[PublicAPI]
public static class WebApplicationBuilderExtensions
{
    public static void AddHouston(
        this WebApplicationBuilder webApplicationBuilder,
        IHostingConfiguration setupEnvironment,
        Action<VostokComponentsSettings>? setupComponentsSettings = null
    )
    {
        webApplicationBuilder.AddVostok();
    }
}