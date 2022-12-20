using Microsoft.Extensions.DependencyInjection;

namespace Vostok.Hosting.AspNetCore.MiddlewareRegistration;

public interface IVostokMiddlewaresBuilder
{
    public IServiceCollection Services { get; }
}