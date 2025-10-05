using System.Text.Json.Serialization;

namespace MoreSpeakers.Domain.Models.Messages;

public class EmailDeliveryReportReceivedData
{
    [JsonPropertyName("sender")]
    public string Sender { get; set; } = string.Empty;
    [JsonPropertyName("recipient")]
    public string Recipient { get; set; } = string.Empty;
    [JsonPropertyName("messageId")]
    public Guid MessageId { get; set; } = Guid.Empty;
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("deliveryStatusDetails")]
    public EmailDeliveryReportReceivedDeliveryStatusDetails DeliveryStatusDetails { get; set; } = new();
    [JsonPropertyName("deliveryAttemptTimestamp")]
    public DateTime DeliveryAttemptTimestamp { get; set; }
}