using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Tests.TestHelpers;

internal class FakeServiceBeacon : IServiceBeacon
{
    private readonly ILog log;

    public FakeServiceBeacon(IReplicaInfo replicaInfo, ILog log)
    {
        ReplicaInfo = replicaInfo;
        this.log = log.ForContext<FakeServiceBeacon>();
    }

    public void Start() =>
        log.Info("Start.");

    public void Stop() =>
        log.Info("Stop.");

    public IReplicaInfo ReplicaInfo { get; }
}