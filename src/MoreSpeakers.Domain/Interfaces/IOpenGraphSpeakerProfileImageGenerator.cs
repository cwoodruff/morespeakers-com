namespace MoreSpeakers.Domain.Interfaces;
using SixLabors.ImageSharp;

public interface IOpenGraphSpeakerProfileImageGenerator
{
    /// <summary>
    /// Queues a message to create an OpenGraph profile image for a speaker.
    /// </summary>
    /// <param name="id">The unique identifier of the speaker.</param>
    /// <param name="headshotUrl">The URL of the speaker's headshot image.</param>
    /// <param name="speakerName">The name of the speaker</param>
    public Task QueueSpeakerOpenGraphProfileImageCreation(Guid id, string headshotUrl, string speakerName);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageUrl">The fully qualified URL to the speaker's image</param>
    /// <param name="logoUrl">The fully qualified URL to the logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <returns>A <see cref="Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="ArgumentException">If any of the urls are not well-formed</exception>
    Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl,
        string speakerName);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageFile">The fully qualified path to the speaker's image file</param>
    /// <param name="logoFile">The fully qualified path to the logo file</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <returns>A <see cref="Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="FileNotFoundException">If any of the files cannot be found</exception>
    Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile,
        string speakerName);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the speaker headshot</param>
    /// <param name="logoImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the MoreSpeaker logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    Image GenerateSpeakerProfile(Image speakerImage, Image logoImage, string speakerName,
        int width = 1200, int height = 630);
}