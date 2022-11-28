using System;
using Vostok.Hosting.Houston;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.AspNetCore.Houston.Helpers;

internal class FakeHoustonHost : HoustonHost
{
    public FakeHoustonHost(Action<IHostingConfiguration> userSetup)
        : base(new FakeVostokApplication(), userSetup)
    {
        
    }

    public VostokHostingEnvironmentSetup EnvironmentSetup =>
        settings.EnvironmentSetup;

    public VostokHostSettings Settings =>
        settings;
}