#nullable disable
using System.Text.Json.Serialization;

namespace MoreSpeakers.Domain.Models.Messages;

public class Data
{
    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("recipient")]
    public string Recipient { get; set; }

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("deliveryStatusDetails")]
    public DeliveryStatusDetails DeliveryStatusDetails { get; set; }

    [JsonPropertyName("deliveryAttemptTimeStamp")]
    public DateTime DeliveryAttemptTimeStamp { get; set; }
}

public class DeliveryStatusDetails
{
    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; set; }
}

public class EmailDeliveryReportReceived
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonPropertyName("data")]
    public Data Data { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; }

    [JsonPropertyName("dataVersion")]
    public string DataVersion { get; set; }

    [JsonPropertyName("metadataVersion")]
    public string MetadataVersion { get; set; }

    [JsonPropertyName("eventTime")]
    public DateTime EventTime { get; set; }
}