namespace MoreSpeakers.Domain.Models;

public class ApplicationInsightsSettings
{
    public bool DisableExceptionTracking { get; set; } = true;
    public bool EnableDebug { get; set; } = false;

    public string ConnectionString { get; set; } = string.Empty;

}