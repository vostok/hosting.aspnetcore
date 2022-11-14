using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Helpers;

internal class VostokHostingEnvironmentKeeper
{
    public IVostokHostingEnvironment Environment { get; }

    public VostokHostingEnvironmentKeeper(IVostokHostingEnvironment environment) =>
        Environment = environment;
}