using System;
using System.Threading.Tasks;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Houston.Helpers;

internal class FakeVostokApplication : IVostokApplication
{
    public Task InitializeAsync(IVostokHostingEnvironment environment) =>
        throw new NotImplementedException("Should not be called.");

    public Task RunAsync(IVostokHostingEnvironment environment) =>
        throw new NotImplementedException("Should not be called.");
}