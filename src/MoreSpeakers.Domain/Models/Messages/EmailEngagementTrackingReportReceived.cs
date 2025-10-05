using System.Text.Json.Serialization;

namespace MoreSpeakers.Domain.Models.Messages;

[Serializable]
public class EmailEngagementTrackingReportReceived: EmailMessageBase
{
    [JsonPropertyName("data")]
    public EmailEngagementTrackingReportReceivedData Data { get; set; } = new();
}