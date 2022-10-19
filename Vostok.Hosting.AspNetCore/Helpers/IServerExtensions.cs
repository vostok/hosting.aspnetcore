using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Vostok.Hosting.Aspnetcore.Helpers;

internal static class IServerExtensions
{
    public static ICollection<string> TryGetAddresses(this IServer server)
    {
        var addressFeature = server.Features.Get<IServerAddressesFeature>();
        return addressFeature?.Addresses;
    }
}