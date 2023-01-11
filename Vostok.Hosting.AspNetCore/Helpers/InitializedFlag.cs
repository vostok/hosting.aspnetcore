using Vostok.Commons.Threading;

namespace Vostok.Hosting.AspNetCore.Helpers;

internal class InitializedFlag : AtomicBoolean
{
    public InitializedFlag()
        : base(false)
    {
    }
}