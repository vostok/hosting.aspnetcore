using System;
using Vostok.Hosting.AspNetCore.Houston.Applications;
using Vostok.Hosting.Houston;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Houston.Helpers;

internal class AspNetCoreHoustonHost : HoustonHost
{
    public AspNetCoreHoustonHost(Action<IHostingConfiguration> userSetup)
        : base(new HoustonApplication(), userSetup)
    {
        
    }

    public VostokHostingEnvironmentSetup EnvironmentSetup =>
        settings.EnvironmentSetup;

    public VostokHostSettings Settings =>
        settings;
}