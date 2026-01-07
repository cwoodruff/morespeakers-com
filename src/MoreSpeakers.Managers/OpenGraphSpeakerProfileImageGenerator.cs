using System.Text.Json;

using Azure.Storage.Blobs;
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

public class OpenGraphSpeakerProfileImageGenerator: IOpenGraphSpeakerProfileImageGenerator
{
    private readonly HttpClient _httpClient;
    private readonly BlobServiceClient _blobClient;
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ISettings _settings;
    private readonly ILogger<OpenGraphSpeakerProfileImageGenerator> _logger;

    public OpenGraphSpeakerProfileImageGenerator(HttpClient httpClient, BlobServiceClient blobClient, QueueServiceClient queueServiceClient, ISettings settings, ILogger<OpenGraphSpeakerProfileImageGenerator> logger)
    {
        _httpClient = httpClient;
        _blobClient = blobClient;
        _queueServiceClient = queueServiceClient;
        _settings = settings;
        _logger = logger;
    }

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
        var queue = new Queue(_queueServiceClient, Domain.Constants.Queues.CreateOpenGraphProfileImage);
        await queue.AddMessageAsync(JsonSerializer.Serialize(message));
        _logger.LogInformation("Queued OpenGraph profile image creation for speaker {Id}", id);
    }


    /// <summary>
    /// The fonts list to try and use when rendering text. The first font that is found will be used.
    /// </summary>
    public string[] ThemeFonts { get; set; } =
    [
        "Ubuntu", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "Helvetica Neue", "Arial", "sans-serif",
        "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"
    ];

    /// <summary>
    /// Loads an image from a URL and returns it as a Stream.
    /// </summary>
    /// <param name="url">The URL of the image.</param>
    /// <returns>A Stream that can be loaded with SixLabors.Image.LoadAsync.</returns>
    private async Task<Stream> GetImageStreamFromUrlAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException(nameof(url));
        }

        try
        {
            var response = await _httpClient.GetAsync(url);
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
    /// <returns>A <see cref="Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="ArgumentException">If any of the urls are not well-formed</exception>
    public async Task<Image?> GenerateSpeakerProfileFromUrlsAsync(string speakerImageUrl, string logoUrl,
        string speakerName)
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

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName);

    }

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImageFile">The fully qualified path to the speaker's image file</param>
    /// <param name="logoFile">The fully qualified path to the logo file</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <returns>A <see cref="Image"/> representing the speaker profile</returns>
    /// <exception cref="ArgumentNullException">If any of the parameters are null or empty</exception>
    /// <exception cref="FileNotFoundException">If any of the files cannot be found</exception>
    public async Task<Image?> GenerateSpeakerProfileFromFilesAsync(string speakerImageFile, string logoFile,
        string speakerName)
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

        return GenerateSpeakerProfile(speakerImage, logoImage, speakerName);
    }

    /// <summary>
    /// Generates a speaker profile image from the speaker image, logo, and a speaker name.
    /// </summary>
    /// <param name="speakerImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the speaker headshot</param>
    /// <param name="logoImage">An <see cref="SixLabors.ImageSharp.Image"/> that represents the MoreSpeaker logo</param>
    /// <param name="speakerName">The name of the speaker</param>
    /// <param name="width">The width of the generated image. The default is <see cref="DefaultOpenGraphWidth"/></param>
    /// <param name="height">The height of the generated image. The default is <see cref="DefaultOpenGraphHeight"/></param>
    /// <returns></returns>
    public Image GenerateSpeakerProfile(
        Image speakerImage,
        Image logoImage,
        string speakerName,
        int width = DefaultOpenGraphWidth,
        int height = DefaultOpenGraphHeight)
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

        // Fonts
        var fontFamilyToUse = GetFontFamilyFromList(ThemeFonts);
        var brandFont = fontFamilyToUse.CreateFont(58, FontStyle.Bold);
        var labelFont = fontFamilyToUse.CreateFont(40, FontStyle.Regular);
        var nameFont = fontFamilyToUse.CreateFont(48, FontStyle.Bold);

        // Text positions
        float textLeft = width / 2 + 40;
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
                WrappingLength = width / 2 - 80
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
    private FontFamily GetFontFamilyFromList(string[] fontFamilyNames, string defaultFontFamily = "Arial")
    {
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
        return fontCollection.Get(defaultFontFamily);
    }
}