using System;
using System.Threading.Tasks;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Houston.Applications;

public class HoustonApplication : IVostokApplication
{
    public Task InitializeAsync(IVostokHostingEnvironment environment) =>
        throw new Exception("Should not be called.");

    public Task RunAsync(IVostokHostingEnvironment environment) =>
        throw new Exception("Should not be called.");
}