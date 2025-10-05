using System.Text.Json.Serialization;

namespace MoreSpeakers.Domain.Models.Messages;

public class EmailEngagementTrackingReportReceivedData  
{
    [JsonPropertyName("sender")]
    public string Sender { get; set; } = string.Empty;
    [JsonPropertyName("messageId")]
    public Guid MessageId { get; set; } = Guid.Empty;
    [JsonPropertyName("userActionTimeStamp")]
    public DateTime UserActionTimeStamp { get; set; }
    [JsonPropertyName("engagementContext")]
    public string EngagementContext { get; set; } = string.Empty;
    [JsonPropertyName("userAgent")]
    public string UserAgent { get; set; } = string.Empty;
    
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Possible values: "View" and "Click". If <see cref="EngagementType"/> is "Click", then <see cref="EngagementContext"/> will contain the URL that was clicked.
    /// </remarks>
    [JsonPropertyName("engagementType")]
    public string EngagementType { get; set; } = string.Empty;
}