using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.Messages;
using JosephGuadagno.AzureHelpers.Storage;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;

namespace MoreSpeakers.Managers;

public class OpenGraphSpeakerProfileImageGenerator(
    HttpClient httpClient,
    QueueServiceClient queueServiceClient,
    ILogger<OpenGraphSpeakerProfileImageGenerator> logger)
    : IOpenGraphSpeakerProfileImageGenerator
{
    /// <summary>
    /// The default value for the width of the generated image.
    /// </summary>
    public const int DefaultOpenGraphWidth = 1200;
    /// <summary>
    /// The default value for the height of the generated image.
    /// </summary>
    public const int DefaultOpenGraphHeight = 630;

    /// <summary>
    /// Queues a message to create an OpenGraph profile image for a speaker.
    /// </summary>
    /// <param name="id">The unique identifier of the speaker.</param>
    /// <param name="headshotUrl">The URL of the speaker's headshot image.</param>
    /// <param name="speakerName">The name of the speaker.</param>
    public async Task QueueSpeakerOpenGraphProfileImageCreation(Guid id, string headshotUrl, string speakerName)
    {
        var message = new CreateOpenGraphProfileImage { UserId = id, ProfileImageUrl = headshotUrl, SpeakerName = speakerName };
        var queue = new Queue(queueServiceClient, Domain.Constants.Queues.CreateOpenGraphProfileImage);
        await queue.AddMessageWithBase64EncodingAsync(message);
        logger.LogInformation("Queued OpenGraph profile image creation for speaker {Id}", id);
    }


    /// <summary>
    /// The fonts list to try and use when rendering text. The first font that is found will be used.
    /// </summary>
    public string[] ThemeFonts { get; } =
    [
        "Ubuntu", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "Helvetica Neue", "Arial", "sans-serif",
        "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"
    ];

    /// <summary>
    /// Loads an image from a URL and returns it as a Stream.
    /// </summary>
    /// <param name="url">The URL of the image.</param>
    /// <returns>A Stream that can be loaded with SixLabors.Image.LoadAsync.</returns>
    /// <exception cref="ArgumentNullException">If the URL is null or empty.</exception>
    /// <exception cref="ArgumentException">If the URL is not well-formed.</exception>
    private async Task<Stream> GetImageStreamFromUrlAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException(nameof(url));
        }
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new ArgumentException("The URL is not well-formed.", nameof(url));
        }

        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

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
    public async Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl,
        string speakerName,
        string[] fontFamilyNames,
        int width = 1200, int height = 630)
    {
        if (string.IsNullOrEmpty(speakerImageUrl))
        {
            throw new ArgumentNullException(nameof(speakerImageUrl));
        }
        if (string.IsNullOrEmpty(logoUrl))
        {
            throw new ArgumentNullException(nameof(logoUrl));
        }
        if (string.IsNullOrEmpty(speakerName))
        {
            throw new ArgumentNullException(nameof(speakerName));
        }
        if (!Uri.IsWellFormedUriString(speakerImageUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Speaker image URL is not well-formed.", nameof(speakerImageUrl));
        }
        if (!Uri.IsWellFormedUriString(logoUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Logo URL is not well-formed.", nameof(logoUrl));
        }
        if (fontFamilyNames == null || fontFamilyNames.Length == 0)
        {
            throw new ArgumentNullException(nameof(fontFamilyNames));
        }

        // Download the images
        await using var speakerImageStream = await GetImageStreamFromUrlAsync(speakerImageUrl);
        await using var logoImageStream = await GetImageStreamFromUrlAsync(logoUrl);

        using var speakerImage = await Image.LoadAsync(speakerImageStream);
        using var logoImage = await Image.LoadAsync(logoImageStream);

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, fontFamilyNames, width, height);
    }

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
    public async Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl,
        string speakerName,
        string fontFamilyFile,
        int width = 1200, int height = 630)
    {
        if (string.IsNullOrEmpty(speakerImageUrl))
        {
            throw new ArgumentNullException(nameof(speakerImageUrl));
        }
        if (string.IsNullOrEmpty(logoUrl))
        {
            throw new ArgumentNullException(nameof(logoUrl));
        }
        if (string.IsNullOrEmpty(speakerName))
        {
            throw new ArgumentNullException(nameof(speakerName));
        }
        if (!Uri.IsWellFormedUriString(speakerImageUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Speaker image URL is not well-formed.", nameof(speakerImageUrl));
        }
        if (!Uri.IsWellFormedUriString(logoUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Logo URL is not well-formed.", nameof(logoUrl));
        }
        if (string.IsNullOrEmpty(fontFamilyFile))
        {
            throw new ArgumentNullException(nameof(fontFamilyFile));
        }
        if (!File.Exists(fontFamilyFile))
        {
            throw new ArgumentException("Font family file does not exist.", nameof(fontFamilyFile));
        }

        // Download the images
        await using var speakerImageStream = await GetImageStreamFromUrlAsync(speakerImageUrl);
        await using var logoImageStream = await GetImageStreamFromUrlAsync(logoUrl);

        using var speakerImage = await Image.LoadAsync(speakerImageStream);
        using var logoImage = await Image.LoadAsync(logoImageStream);

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, fontFamilyFile, width, height);
    }

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
    public async Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl,
        string speakerName,
        FontFamily fontFamily,
        int width = 1200, int height = 630)
    {
        if (string.IsNullOrEmpty(speakerImageUrl))
        {
            throw new ArgumentNullException(nameof(speakerImageUrl));
        }
        if (string.IsNullOrEmpty(logoUrl))
        {
            throw new ArgumentNullException(nameof(logoUrl));
        }
        if (string.IsNullOrEmpty(speakerName))
        {
            throw new ArgumentNullException(nameof(speakerName));
        }
        if (!Uri.IsWellFormedUriString(speakerImageUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Speaker image URL is not well-formed.", nameof(speakerImageUrl));
        }
        if (!Uri.IsWellFormedUriString(logoUrl, UriKind.Absolute))
        {
            throw new ArgumentException("Logo URL is not well-formed.", nameof(logoUrl));
        }
        // Download the images
        await using var speakerImageStream = await GetImageStreamFromUrlAsync(speakerImageUrl);
        await using var logoImageStream = await GetImageStreamFromUrlAsync(logoUrl);

        using var speakerImage = await Image.LoadAsync(speakerImageStream);
        using var logoImage = await Image.LoadAsync(logoImageStream);

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, fontFamily, width, height);
    }

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
    public async Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile,
        string speakerName,
        string[] fontFamilyNames,
        int width = 1200, int height = 630)
    {
        if (string.IsNullOrEmpty(speakerImageFile))
        {
            throw new ArgumentNullException(nameof(speakerImageFile));
        }
        if (string.IsNullOrEmpty(logoFile))
        {
            throw new ArgumentNullException(nameof(logoFile));
        }
        if (string.IsNullOrEmpty(speakerName))
        {
            throw new ArgumentNullException(nameof(speakerName));
        }
        if (!File.Exists(speakerImageFile))
        {
            throw new FileNotFoundException("Speaker image file not found.", speakerImageFile);
        }
        if (!File.Exists(logoFile))
        {
            throw new FileNotFoundException("Logo file not found.", logoFile);
        }
        if (fontFamilyNames == null || fontFamilyNames.Length == 0)
        {
            throw new ArgumentNullException(nameof(fontFamilyNames));
        }

        using var speakerImage = await Image.LoadAsync(speakerImageFile);
        using var logoImage = await Image.LoadAsync(logoFile);

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, fontFamilyNames, width, height);
    }

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
    public async Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile,
        string speakerName,
        string fontFamilyFile,
        int width = 1200, int height = 630)
    {
        if (string.IsNullOrEmpty(speakerImageFile))
        {
            throw new ArgumentNullException(nameof(speakerImageFile));
        }
        if (string.IsNullOrEmpty(logoFile))
        {
            throw new ArgumentNullException(nameof(logoFile));
        }
        if (string.IsNullOrEmpty(speakerName))
        {
            throw new ArgumentNullException(nameof(speakerName));
        }
        if (!File.Exists(speakerImageFile))
        {
            throw new FileNotFoundException("Speaker image file not found.", speakerImageFile);
        }
        if (!File.Exists(logoFile))
        {
            throw new FileNotFoundException("Logo file not found.", logoFile);
        }
        if (!File.Exists(fontFamilyFile))
        {
            throw new FileNotFoundException("Font family file not found.", fontFamilyFile);
        }

        using var speakerImage = await Image.LoadAsync(speakerImageFile);
        using var logoImage = await Image.LoadAsync(logoFile);

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, fontFamilyFile, width, height);
    }

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
    public async Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile,
        string speakerName,
        FontFamily fontFamily,
        int width = 1200, int height = 630)
    {
        if (string.IsNullOrEmpty(speakerImageFile))
        {
            throw new ArgumentNullException(nameof(speakerImageFile));
        }
        if (string.IsNullOrEmpty(logoFile))
        {
            throw new ArgumentNullException(nameof(logoFile));
        }
        if (string.IsNullOrEmpty(speakerName))
        {
            throw new ArgumentNullException(nameof(speakerName));
        }
        if (!File.Exists(speakerImageFile))
        {
            throw new FileNotFoundException("Speaker image file not found.", speakerImageFile);
        }
        if (!File.Exists(logoFile))
        {
            throw new FileNotFoundException("Logo file not found.", logoFile);
        }

        using var speakerImage = await Image.LoadAsync(speakerImageFile);
        using var logoImage = await Image.LoadAsync(logoFile);

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, fontFamily, width, height);
    }

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
    public Image GenerateSpeakerProfile(Image speakerImage, Image logoImage, string speakerName,
        string[] fontFamilyNames,
        int width = 1200, int height = 630)
    {
        var fontFamilyToUse = GetFontFamilyFromList(fontFamilyNames);
        if (fontFamilyToUse == null)
        {
            throw new ApplicationException("No fonts found in the list of fonts provided.");
        }
        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, (FontFamily)fontFamilyToUse.Value, width, height);
    }

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
    public Image GenerateSpeakerProfile(Image speakerImage, Image logoImage, string speakerName,
        string fontFamilyFile,
        int width = 1200, int height = 630)
    {
        var fontFamilyToUse = GetFontFamilyFromFile(fontFamilyFile);
        if (fontFamilyToUse == null)
        {
            throw new ApplicationException("No fonts found in the list of fonts provided.");
        }
        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName, (FontFamily)fontFamilyToUse.Value, width, height);
    }

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the speaker headshot</param>
    /// <param name="logoImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the MoreSpeaker logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="fontFamily">The <see cref="SixLabors.Fonts.FontFamily"/> to use for the speaker name and other text elements</param>
    /// <param name="width">The width of the generated image.</param>
    /// <param name="height">The height of the generated image.</param>
    public Image GenerateSpeakerProfile(Image speakerImage, Image logoImage, string speakerName,
        FontFamily fontFamily,
        int width = 1200, int height = 630)
    {
using var canvas = new Image<Rgba32>(width, height);

        // Gradient background (Bootstrap United style)
        var gradientBrush = new LinearGradientBrush(
            new PointF(0, 0),
            new PointF(width, height),
            GradientRepetitionMode.None,
            new[]
            {
                new ColorStop(0f, Color.ParseHex("#E95420")), // orange-red
                new ColorStop(1f, Color.ParseHex("#F7C873"))  // warm yellow
            });

        canvas.Mutate(ctx => ctx.Fill(gradientBrush));

        // Adjust the speaker image
        speakerImage.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width / 2, height),
            Mode = ResizeMode.Crop
        }));

        // Paste speaker image on left
        canvas.Mutate(ctx => ctx.DrawImage(speakerImage, new Point(0, 0), 1f));

        // Logo
        var logoWidth = 110;
        var logoHeight = 110;
        logoImage.Mutate(x => x.Resize(logoWidth, logoHeight));

        // Paste the logo in the center of the width of the gradient background
        canvas.Mutate(ctx => ctx.DrawImage(logoImage, new Point((width / 2 / 2 - logoWidth / 2) + width / 2, 40), 1f));

        // Generate the text elements
        var brandFont = fontFamily.CreateFont(58, FontStyle.Bold);
        var labelFont = fontFamily.CreateFont(40, FontStyle.Regular);
        var nameFont = fontFamily.CreateFont(48, FontStyle.Bold);

        // Text positions
        float textLeft = (float)width / 2 + 40;
        float brandTop = 200;
        float labelTop = brandTop + 70;
        float nameTop = labelTop + 90;

        canvas.Mutate(ctx =>
        {
            // MoreSpeakers
            ctx.DrawText(new RichTextOptions(brandFont)
            {
                Origin = new PointF(textLeft, brandTop),
                HorizontalAlignment = HorizontalAlignment.Left
            }, "MoreSpeakers.com", Color.White);

            // Speaker Profile
            ctx.DrawText(new RichTextOptions(labelFont)
            {
                Origin = new PointF(textLeft, labelTop),
                HorizontalAlignment = HorizontalAlignment.Left
            }, "Speaker Profile", Color.White);

            // Speaker Name (dynamic)
            ctx.DrawText(new RichTextOptions(nameFont)
            {
                Origin = new PointF(textLeft, nameTop),
                HorizontalAlignment = HorizontalAlignment.Left,
                WrappingLength = (float) width / 2 - 80
            }, speakerName, Color.White);
        });

        // return the final image
        Image image = canvas.Clone();
        return image;
    }

    /// <summary>
    /// Returns the first FontFamily that matches the given name in the list of installed fonts.
    /// </summary>
    /// <param name="fontFamilyNames">Any array of font family names to search</param>
    /// <param name="defaultFontFamily">The default font family to use if none are found</param>
    /// <returns>The first matching FontFamily or <paramref name="defaultFontFamily"/> if none found</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fontFamilyNames"/> is null</exception>
    /// <exception cref="SixLabors.Fonts.FontFamilyNotFoundException">If none of the fonts in <paramref name="fontFamilyNames"/> are found or the default font was not found.</exception>
    public FontFamily? GetFontFamilyFromList(string[] fontFamilyNames, string defaultFontFamily = "Arial")
    {
        if (fontFamilyNames == null)
        {
            throw new ArgumentNullException(nameof(fontFamilyNames), "Font family names cannot be null");
        }

        var fontCollection = new FontCollection();
        fontCollection.AddSystemFonts();
        foreach (var fontFamilyName in fontFamilyNames)
        {
            try
            {
                var fontFamily = fontCollection.Get(fontFamilyName);
                return fontFamily;
            }
            catch
            {
                // Do nothing, we'll cycle through the list of fonts until we find one that works
            }
        }

        try
        {
            return fontCollection.Get(defaultFontFamily);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading fonts");
            return null;
        }
    }

    /// <summary>
    /// Returns the FontFamily that was loaded from the given file.
    /// </summary>
    /// <param name="fontFamilyFile">The path to the font file</param>
    /// <returns>The FontFamily </returns>
    /// <exception cref="ArgumentNullException">If <paramref name="fontFamilyFile"/> is null or empty</exception>
    /// <exception cref="System.IO.FileNotFoundException">If the file cannot be found</exception>
    public FontFamily? GetFontFamilyFromFile(string fontFamilyFile)
    {
        if (string.IsNullOrEmpty(fontFamilyFile))
        {
            throw new ArgumentNullException(nameof(fontFamilyFile));
        }
        if (!File.Exists(fontFamilyFile))
        {
            throw new FileNotFoundException($"Font file not found: {fontFamilyFile}", fontFamilyFile);
        }

        try
        {
            return new FontCollection().Add(fontFamilyFile);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading fonts from file \'{FontFamilyFile}\'", fontFamilyFile);
            return null;
        }
    }
}