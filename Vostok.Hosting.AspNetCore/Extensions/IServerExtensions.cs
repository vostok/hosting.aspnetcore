using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Vostok.Hosting.AspNetCore.Extensions;

internal static class IServerExtensions
{
    public static ICollection<string>? TryGetAddresses(this IServer server)
    {
        var addressFeature = server.Features.Get<IServerAddressesFeature>();
        return addressFeature?.Addresses;
    }
}