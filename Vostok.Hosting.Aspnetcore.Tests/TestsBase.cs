using NUnit.Framework;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Vostok.Hosting.Aspnetcore.Tests;

internal class TestsBase
{
    protected int Port { get; private set; }
    protected IClusterClient Client { get; private set; }
    protected ILog Log { get; private set; }
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Log = new CompositeLog(
            new SynchronousConsoleLog(),
            new FileLog(new FileLogSettings
            {
                FileOpenMode = FileOpenMode.Rewrite
            }));

        Port = FreeTcpPortFinder.GetFreePort();
        
        Client = CreateClusterClient();
    }

    protected void SetupEnvironmentDefaults(IVostokHostingEnvironmentBuilder builder)
    {
        builder
            .SetupApplicationIdentity(
                s => s.SetProject("Project")
                    .SetSubproject("Subproject")
                    .SetEnvironment("Environment")
                    .SetApplication("Application")
                    .SetInstance("Instance"))
            .SetPort(Port)
            .SetupLog(s => s.AddLog(Log));

        builder.DisableClusterConfig();
    }
    
    private IClusterClient CreateClusterClient()
    {
        // ReSharper disable once RedundantNameQualifier
        // full type name currently required due to https://github.com/vostok/clusterclient.datacenters/issues/1
        return new Clusterclient.Core.ClusterClient(
            Log,
            s =>
            {
                s.ClusterProvider = new FixedClusterProvider($"http://localhost:{Port}");
                s.SetupUniversalTransport();
            });
    }
}