using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Domain.Models;

/// <summary>
/// The Open Graph settings.
/// </summary>
public class OpenGraphSettings:IOpenGraphSettings
{
    /// <summary>
    /// The Open Graph speaker card blob URL.
    /// </summary>
    public required string SpeakerCardBlobUrl { get; init; }
}