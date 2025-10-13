using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Domain.Models;

/// <summary>
/// Settings for the application.
/// </summary>
public class Settings : ISettings
{
    /// <summary>
    /// The email settings.
    /// </summary>
    public required EmailSettings Email { get; init; }
}