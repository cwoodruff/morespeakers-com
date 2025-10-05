using System.Text.Json.Serialization;

namespace MoreSpeakers.Domain.Models.Messages;

public class EmailMessageBase
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.Empty;
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;
    [JsonPropertyName("dataVersion")]
    public string DataVersion { get; set; } = string.Empty;
    [JsonPropertyName("metadataVersion")]
    public string MetadataVersion { get; set; } = string.Empty;
    [JsonPropertyName("eventTime")]
    public DateTime EventTime { get; set; }
}