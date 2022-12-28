using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Helpers;

internal class BuiltVostokEnvironment
{
    public BuiltVostokEnvironment(IVostokHostingEnvironment environment) =>
        Environment = environment;

    public IVostokHostingEnvironment Environment { get; }
}