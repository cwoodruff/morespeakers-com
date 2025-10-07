namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Email settings.
/// </summary>
public interface IEmailSettings
{
    /// <summary>
    /// The from address for emails.
    /// </summary>
    public string FromAddress { get; init; }
    
    /// <summary>
    /// The from name for emails.
    /// </summary>
    public string FromName { get; init; }
    
    /// <summary>
    /// The reply to address for emails.
    /// </summary>
    public string ReplyToAddress { get; init; }
    
    /// <summary>
    /// The reply to name for emails.
    /// </summary>
    public string ReplyToName { get; init; }
}