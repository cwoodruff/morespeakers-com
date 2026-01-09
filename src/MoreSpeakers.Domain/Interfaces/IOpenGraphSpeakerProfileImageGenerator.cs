using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Generates speaker profile images from speaker images, logos, and speaker names.
/// </summary>
public interface IOpenGraphSpeakerProfileImageGenerator
{

    /// <summary>
    /// Queues a message to create an OpenGraph profile image for a speaker.
    /// </summary>
    /// <param name="id">The unique identifier of the speaker.</param>
    /// <param name="headshotUrl">The URL of the speaker's headshot image.</param>
    /// <param name="speakerName">The name of the speaker.</param>
    Task QueueSpeakerOpenGraphProfileImageCreation(Guid id, string headshotUrl, string speakerName);

    /// <summary>
    /// The fonts list to try and use when rendering text. The first font that is found will be used.
    /// </summary>
    string[] ThemeFonts { get; }

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageUrl">The fully qualified URL to the speaker's image</param>
    /// <param name="logoUrl">The fully qualified URL to the logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamilyNames">A string array of font names to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>A <see cref="SixLabors.ImageSharp.Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="ArgumentException">If any of the urls are not well-formed</exception>
    Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl, string speakerName,
        string[] fontFamilyNames,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageUrl">The fully qualified URL to the speaker's image</param>
    /// <param name="logoUrl">The fully qualified URL to the logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamilyFile">The fully qualified font file to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>A <see cref="SixLabors.ImageSharp.Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="ArgumentException">If any of the urls are not well-formed</exception>
    /// <exception cref="FileNotFoundException">If the font file cannot be found</exception>
    Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl, string speakerName,
        string fontFamilyFile,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageUrl">The fully qualified URL to the speaker's image</param>
    /// <param name="logoUrl">The fully qualified URL to the logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamily">The <see cref="SixLabors.Fonts.FontFamily"/> to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>A <see cref="SixLabors.ImageSharp.Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="ArgumentException">If any of the urls are not well-formed</exception>
    Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl, string speakerName,
        FontFamily fontFamily,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageFile">The fully qualified path to the speaker's image file</param>
    /// <param name="logoFile">The fully qualified path to the logo file</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamilyNames">A string array of font names to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>A <see cref="SixLabors.ImageSharp.Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="FileNotFoundException">If any of the files cannot be found</exception>
    Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile, string speakerName,
        string[] fontFamilyNames,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageFile">The fully qualified path to the speaker's image file</param>
    /// <param name="logoFile">The fully qualified path to the logo file</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamilyFile">The fully qualified font file to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>A <see cref="SixLabors.ImageSharp.Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="FileNotFoundException">If any of the files cannot be found</exception>
    Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile, string speakerName,
        string fontFamilyFile,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageFile">The fully qualified path to the speaker's image file</param>
    /// <param name="logoFile">The fully qualified path to the logo file</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamily">The <see cref="SixLabors.Fonts.FontFamily"/> to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>A <see cref="SixLabors.ImageSharp.Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="FileNotFoundException">If any of the files cannot be found</exception>
    Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile, string speakerName,
        FontFamily fontFamily,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the speaker headshot</param>
    /// <param name="logoImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the MoreSpeaker logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamilyNames">A string array of font names to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>The generated image</returns>
    Image? GenerateSpeakerProfile(Image speakerImage, Image logoImage, string speakerName,
        string[] fontFamilyNames,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the speaker headshot</param>
    /// <param name="logoImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the MoreSpeaker logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamilyFile">The fully qualified font file to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    /// <returns>The generated image</returns>
    Image? GenerateSpeakerProfile(Image speakerImage, Image logoImage, string speakerName,
        string fontFamilyFile,
        int width = 1200, int height = 630);

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the speaker headshot</param>
    /// <param name="logoImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the MoreSpeaker logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamily">The <see cref="SixLabors.Fonts.FontFamily"/> to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    Image? GenerateSpeakerProfile(Image speakerImage, Image logoImage, string speakerName,
        FontFamily fontFamily,
        int width = 1200, int height = 630);

    /// <summary>
    /// Returns the first FontFamily that matches the given name in the list of installed fonts.
    /// </summary>
    /// <param name="fontFamilyNames">Any array of font family names to search</param>
    /// <param name="defaultFontFamily">The default font family to use if none are found</param>
    /// <returns>The first matching FontFamily or <paramref name="defaultFontFamily"/> if none found</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fontFamilyNames"/> is null</exception>
    /// <exception cref="SixLabors.Fonts.FontFamilyNotFoundException">If none of the fonts in <paramref name="fontFamilyNames"/> are found or default are found.</exception>
    FontFamily? GetFontFamilyFromList(string[] fontFamilyNames, string defaultFontFamily = "Arial");

    /// <summary>
    /// Returns the FontFamily that was loaded from the given file.
    /// </summary>
    /// <param name="fontFamilyFile">The path to the font file</param>
    /// <returns>The FontFamily </returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fontFamilyFile"/> is null or empty</exception>
    /// <exception cref="System.IO.FileNotFoundException">If the file cannot be found</exception>
    FontFamily? GetFontFamilyFromFile(string fontFamilyFile);
}