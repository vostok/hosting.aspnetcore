using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;
using Vostok.Logging.Formatting;

namespace Vostok.Hosting.AspNetCore.Tests.TestHelpers;

internal class CheckingLog : ILog
{
    private readonly LinkedList<string> expectedMessages;

    public CheckingLog(params string?[] expectedMessages) =>
        this.expectedMessages = new LinkedList<string>(expectedMessages.Where(m => m != null)!);

    public void Log(LogEvent @event)
    {
        if (@event == null)
            return;
        
        var str = LogEventFormatter.Format(@event, OutputTemplate.Default);
        
        lock (expectedMessages)
            if (expectedMessages.Any() && str.Contains(expectedMessages.First()))
                expectedMessages.RemoveFirst();
    }

    public bool IsEnabledFor(LogLevel level) =>
        true;

    public ILog ForContext(string context) =>
        new SourceContextWrapper(this, context);

    public void EnsureReceivedExpectedMessages()
    {
        lock (expectedMessages)
            if (expectedMessages.Any())
                throw new Exception($"Haven't received message '{expectedMessages.First()}'.");
    }
}