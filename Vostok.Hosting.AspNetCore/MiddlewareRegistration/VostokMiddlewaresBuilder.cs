using Microsoft.Extensions.DependencyInjection;

namespace Vostok.Hosting.AspNetCore.MiddlewareRegistration;

internal sealed class VostokMiddlewaresBuilder : IVostokMiddlewaresBuilder
{
    public VostokMiddlewaresBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}