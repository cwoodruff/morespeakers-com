using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Domain.Models;

/// <summary>
/// The settings for the email sender.
/// </summary>
public class EmailSettings: IEmailSettings
{
    /// <summary>
    /// The from address for emails.
    /// </summary>
    public required string FromAddress { get; init; }
    
    /// <summary>
    /// The from name for emails.
    /// </summary>
    public required string FromName { get; init; }
    
    /// <summary>
    /// The reply to address for emails.
    /// </summary>
    public required string ReplyToAddress { get; init; }
    
    /// <summary>
    /// The reply to name for emails.
    /// </summary>
    public required string ReplyToName { get; init; }
}