using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Vostok.Hosting.Aspnetcore.Helpers;

internal static class IHostApplicationLifetimeExtensions
{
    public static async Task<bool> TryWaitStartedAsync(this IHostApplicationLifetime lifetime, CancellationToken cancellationToken)
    {
        var applicationStartedSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var _ = lifetime.ApplicationStarted.Register(() => applicationStartedSignal.TrySetResult());
        await using var __ = cancellationToken.Register(() => cancellationSignal.TrySetResult());

        var completedTask = await Task.WhenAny(applicationStartedSignal.Task, cancellationSignal.Task).ConfigureAwait(false);

        return completedTask == applicationStartedSignal.Task;
    }
}