namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// The Open Graph settings.
/// </summary>
public interface IOpenGraphSettings
{
    /// <summary>
    /// The Open Graph speaker card blob URL.
    /// </summary>
    public string SpeakerCardBlobUrl { get; init; }
}