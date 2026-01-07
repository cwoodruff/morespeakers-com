namespace MoreSpeakers.Domain.Models.Messages;

/// <summary>
/// The message to create an Open Graph profile image
/// </summary>
public class CreateOpenGraphProfileImage
{
    /// <summary>
    /// The user ID
    /// </summary>
    /// <remarks>
    /// This will be used to for the name of the file. It will be {id].png
    /// </remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// The URL of the profile image.
    /// </summary>
    public required string ProfileImageUrl { get; set; }

    /// <summary>
    /// The speaker's name.
    /// </summary>
    public required string SpeakerName { get; set; }

}