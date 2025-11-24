using Microsoft.ApplicationInsights.Channel;

namespace MoreSpeakers.Web.Tests.Utility;

public class FakeTelemetryChannel : ITelemetryChannel
{
    public List<ITelemetry> SentTelemetries { get; } = [];
    public void Send(ITelemetry item)
    {
        SentTelemetries.Add(item);
    }
    public void Flush()
    {
        // No-op
    }
    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; } = string.Empty;
    public void Dispose()
    {
        // No-op
    }
}