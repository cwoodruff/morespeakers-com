using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Settings for the application.
/// </summary>
public interface ISettings
{   
    /// <summary>
    /// The email settings.
    /// </summary>
    public EmailSettings Email { get; init; }
}