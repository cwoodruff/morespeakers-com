using System.Text.Json.Serialization;

namespace MoreSpeakers.Domain.Models.Messages;

public class EmailDeliveryReportReceivedDeliveryStatusDetails
{
    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; set; } = string.Empty;
}