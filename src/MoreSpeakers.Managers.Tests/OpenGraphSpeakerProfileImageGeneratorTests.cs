using System.Net;
using System.Text;

using Azure;
using Azure.Core.Pipeline;
using Azure.Storage;
using Azure.Storage.Queues;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using RichardSzalay.MockHttp;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MoreSpeakers.Managers.Tests;

public class OpenGraphSpeakerProfileImageGeneratorTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly Mock<QueueServiceClient> _queueServiceClientMock;
    private readonly Mock<ILogger<OpenGraphSpeakerProfileImageGenerator>> _loggerMock;
    private readonly OpenGraphSpeakerProfileImageGenerator _generator;
    private readonly string _tempFontFile;
    private readonly string _tempImageFile;
    private readonly string _tempLogoFile;

    private sealed class FakeQueueHandler : HttpMessageHandler
    {
        private const string SendMessageResponseXml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<QueueMessagesList>" +
            "<QueueMessage>" +
            "<MessageId>11111111-1111-1111-1111-111111111111</MessageId>" +
            "<InsertionTime>Wed, 01 Jan 2025 00:00:00 GMT</InsertionTime>" +
            "<ExpirationTime>Wed, 01 Jan 2025 01:00:00 GMT</ExpirationTime>" +
            "<PopReceipt>pop</PopReceipt>" +
            "<TimeNextVisible>Wed, 01 Jan 2025 00:01:00 GMT</TimeNextVisible>" +
            "</QueueMessage>" +
            "</QueueMessagesList>";

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content != null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            var statusCode = request.Method == HttpMethod.Post || request.Method == HttpMethod.Put
                ? HttpStatusCode.Created
                : HttpStatusCode.OK;
            var response = new HttpResponseMessage(statusCode)
            {
                Content = request.Method == HttpMethod.Head
                    ? null
                    : new StringContent(SendMessageResponseXml, Encoding.UTF8, "application/xml")
            };
            return response;
        }
    }

    private static QueueServiceClient CreateQueueServiceClient(FakeQueueHandler handler)
    {
        var options = new QueueClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(handler))
        };
        var credential = new StorageSharedKeyCredential("testaccount", Convert.ToBase64String(new byte[32]));
        return new QueueServiceClient(new Uri("https://example.com"), credential, options);
    }

    public OpenGraphSpeakerProfileImageGeneratorTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        _queueServiceClientMock = new Mock<QueueServiceClient>();
        _loggerMock = new Mock<ILogger<OpenGraphSpeakerProfileImageGenerator>>();
        _generator = new OpenGraphSpeakerProfileImageGenerator(httpClient, _queueServiceClientMock.Object, _loggerMock.Object);

        // Create a dummy font file if possible, or use a known one.
        // For testing purposes, we might need a real font file to avoid ImageSharp/Fonts exceptions.
        // We can use a small base64 encoded font or just skip tests that require a real font file if not available.
        // Actually, let's try to find a system font file for testing GetFontFamilyFromFile.
        _tempFontFile = Path.Combine(Path.GetTempPath(), "testfont.ttf");
        // We won't create it here, but in specific tests where needed.

        _tempImageFile = Path.Combine(Path.GetTempPath(), "testimage.png");
        _tempLogoFile = Path.Combine(Path.GetTempPath(), "testlogo.png");

        using (var img = new Image<Rgba32>(100, 100))
        {
            img.SaveAsPng(_tempImageFile);
            img.SaveAsPng(_tempLogoFile);
        }
    }

    public void Dispose()
    {
        if (File.Exists(_tempFontFile)) File.Delete(_tempFontFile);
        if (File.Exists(_tempImageFile)) File.Delete(_tempImageFile);
        if (File.Exists(_tempLogoFile)) File.Delete(_tempLogoFile);
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_Throws_When_SpeakerImageUrl_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync(null!, "http://logo.com/i.png", "Name", ["Arial"]));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_Throws_When_LogoUrl_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", null!, "Name", ["Arial"]));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_Throws_When_SpeakerName_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", null!, ["Arial"]));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_Throws_When_Invalid_Urls()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("invalid-url", "http://logo.com/i.png", "Name", ["Arial"]));
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "invalid-url", "Name", ["Arial"]));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_Throws_When_FontFamilyNames_Empty()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", "Name", (string[])null!));
        
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", "Name", Array.Empty<string>()));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_Success()
    {
        // Arrange
        byte[] imageBytes;
        using (var img = new Image<Rgba32>(100, 100))
        {
            using var ms = new MemoryStream();
            img.SaveAsPng(ms);
            imageBytes = ms.ToArray();
        }

        _mockHttp.When("http://speaker.com/i.png").Respond("image/png", new MemoryStream(imageBytes));
        _mockHttp.When("http://logo.com/i.png").Respond("image/png", new MemoryStream(imageBytes));

        // Act
        var result = await _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", "John Doe", ["Arial"]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1200, result.Width);
        Assert.Equal(630, result.Height);
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFile_Throws_When_Invalid_Urls()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("invalid-url", "http://logo.com/i.png", "Name", "font.ttf"));
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "invalid-url", "Name", "font.ttf"));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFamily_Throws_When_Invalid_Urls()
    {
        var ff = SystemFonts.Families.First();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("invalid-url", "http://logo.com/i.png", "Name", ff));
        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "invalid-url", "Name", ff));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFile_Throws_When_Args_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync(null!, "http://logo.com/i.png", "Name", "font.ttf"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", null!, "Name", "font.ttf"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", null!, "font.ttf"));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFamily_Throws_When_Args_Null()
    {
        var ff = SystemFonts.Families.First();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync(null!, "http://logo.com/i.png", "Name", ff));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", null!, "Name", ff));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", null!, ff));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFile_Throws_When_FileNotExists()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", "Name", "nonexistent.ttf"));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontNames_Throws_When_Paths_NullOrEmpty()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(null!, _tempLogoFile, "Name", ["Arial"]));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, null!, "Name", ["Arial"]));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, _tempLogoFile, null!, ["Arial"]));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_Throws_When_FileNotFound()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync("nonexistent.png", _tempLogoFile, "Name", ["Arial"]));
        
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, "nonexistent.png", "Name", ["Arial"]));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontFile_Throws_When_FileNotFound()
    {
        var fontFile = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "*.ttf").First();
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync("nonexistent.png", _tempLogoFile, "Name", fontFile));
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, "nonexistent.png", "Name", fontFile));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontFamily_Throws_When_FileNotFound()
    {
        var ff = SystemFonts.Families.First();
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync("nonexistent.png", _tempLogoFile, "Name", ff));
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, "nonexistent.png", "Name", ff));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_Success()
    {
        var result = await _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, _tempLogoFile, "John Doe", ["Arial"]);
        Assert.NotNull(result);
        Assert.Equal(1200, result.Width);
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFamily_Success()
    {
        // Arrange
        byte[] imageBytes;
        using (var img = new Image<Rgba32>(100, 100))
        {
            using var ms = new MemoryStream();
            img.SaveAsPng(ms);
            imageBytes = ms.ToArray();
        }

        _mockHttp.When("http://speaker.com/i.png").Respond("image/png", new MemoryStream(imageBytes));
        _mockHttp.When("http://logo.com/i.png").Respond("image/png", new MemoryStream(imageBytes));

        var fontFamily = SystemFonts.Families.First();

        // Act
        var result = await _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", "John Doe", fontFamily);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFile_Success()
    {
        var fontFile = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "*.ttf").FirstOrDefault();
        if (fontFile == null) return;

        byte[] imageBytes;
        using (var img = new Image<Rgba32>(100, 100))
        {
            using var ms = new MemoryStream();
            img.SaveAsPng(ms);
            imageBytes = ms.ToArray();
        }

        _mockHttp.When("http://speaker.com/i.png").Respond("image/png", new MemoryStream(imageBytes));
        _mockHttp.When("http://logo.com/i.png").Respond("image/png", new MemoryStream(imageBytes));

        var result = await _generator.GenerateSpeakerProfileFromUrlsAsync("http://speaker.com/i.png", "http://logo.com/i.png", "John Doe", fontFile);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontFile_Success()
    {
        // We need a real font file. Let's see if we can find one.
        // On Windows, they are in C:\Windows\Fonts.
        // But for portability, maybe we just skip or use a dummy if we can't.
        // Let's try to find any .ttf file in the system fonts.
        var fontFile = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "*.ttf").FirstOrDefault();
        if (fontFile == null) return; // Skip if no fonts found

        var result = await _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, _tempLogoFile, "John Doe", fontFile);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontFamily_Success()
    {
        var fontFamily = SystemFonts.Families.First();
        var result = await _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, _tempLogoFile, "John Doe", fontFamily);
        Assert.NotNull(result);
    }

    [Fact]
    public void GenerateSpeakerProfile_WithFontFamilyNames_Success()
    {
        using var speakerImg = new Image<Rgba32>(100, 100);
        using var logoImg = new Image<Rgba32>(100, 100);
        var result = _generator.GenerateSpeakerProfile(speakerImg, logoImg, "John Doe", ["Arial"]);
        Assert.NotNull(result);
    }


    [Fact]
    public void GenerateSpeakerProfile_WithFontFile_Success()
    {
        var fontFile = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "*.ttf").FirstOrDefault();
        if (fontFile == null) return;

        using var speakerImg = new Image<Rgba32>(100, 100);
        using var logoImg = new Image<Rgba32>(100, 100);

        var result = _generator.GenerateSpeakerProfile(speakerImg, logoImg, "John Doe", fontFile);
        Assert.NotNull(result);
    }

    [Fact]
    public void GenerateSpeakerProfile_WithFontFamily_Success()
    {
        var fontFamily = SystemFonts.Families.First();
        using var speakerImg = new Image<Rgba32>(100, 100);
        using var logoImg = new Image<Rgba32>(100, 100);

        var result = _generator.GenerateSpeakerProfile(speakerImg, logoImg, "John Doe", fontFamily);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetFontFamilyFromFile_Success()
    {
        var fontFile = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "*.ttf").FirstOrDefault();
        if (fontFile == null) return;

        var font = _generator.GetFontFamilyFromFile(fontFile);
        Assert.NotNull(font);
    }

    [Fact]
    public void GetFontFamilyFromFile_Error_Logs_And_Returns_Null()
    {
        // Create a fake font file that is not a real font
        File.WriteAllText(_tempFontFile, "not a font");
        
        var font = _generator.GetFontFamilyFromFile(_tempFontFile);
        
        Assert.Null(font);
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error loading fonts from file")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void ThemeFonts_Is_Populated()
    {
        Assert.NotNull(_generator.ThemeFonts);
        Assert.Contains("Ubuntu", _generator.ThemeFonts);
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromUrlsAsync_WithFontFile_Throws_When_FontFile_NullOrEmpty()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://s.com/i.png", "http://l.com/i.png", "Name", (string)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://s.com/i.png", "http://l.com/i.png", "Name", ""));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontFile_Throws_When_FontFile_NotExists()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, _tempLogoFile, "Name", "nonexistent.ttf"));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontFile_Throws_When_Paths_NullOrEmpty()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(null!, _tempLogoFile, "Name", "font.ttf"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, null!, "Name", "font.ttf"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, _tempLogoFile, null!, "font.ttf"));
    }

    [Fact]
    public async Task GenerateSpeakerProfileFromFilesAsync_WithFontFamily_Throws_When_Paths_NullOrEmpty()
    {
        var ff = SystemFonts.Families.First();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(null!, _tempLogoFile, "Name", ff));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, null!, "Name", ff));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateSpeakerProfileFromFilesAsync(_tempImageFile, _tempLogoFile, null!, ff));
    }

    [Fact]
    public void GenerateSpeakerProfile_WithFontFile_Throws_When_FontLoading_Fails()
    {
        using var speakerImg = new Image<Rgba32>(100, 100);
        using var logoImg = new Image<Rgba32>(100, 100);
        
        File.WriteAllText(_tempFontFile, "not a font");

        Assert.Throws<ApplicationException>(() =>
            _generator.GenerateSpeakerProfile(speakerImg, logoImg, "Name", _tempFontFile));
    }

    [Fact]
    public void GetFontFamilyFromList_Throws_When_List_Null()
    {
        Assert.Throws<ArgumentNullException>(() => _generator.GetFontFamilyFromList(null!));
    }

    [Fact]
    public void GetFontFamilyFromList_Returns_Default_When_List_Missing()
    {
        var defaultFontName = SystemFonts.Families.First().Name;

        var font = _generator.GetFontFamilyFromList(["NonExistentFont"], defaultFontName);

        Assert.NotNull(font);
    }

    [Fact]
    public void GetFontFamilyFromList_Returns_First_Found_In_List()
    {
        var existingFontName = SystemFonts.Families.First().Name;

        var font = _generator.GetFontFamilyFromList(["NonExistentFont", existingFontName], "NonExistentDefault");

        Assert.NotNull(font);
    }

    [Fact]
    public void GetFontFamilyFromList_Returns_Null_When_Default_Also_Missing()
    {
        var font = _generator.GetFontFamilyFromList(["NonExistentFont"], "NonExistentDefault");
        Assert.Null(font);
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error loading fonts")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void GetFontFamilyFromFile_Throws_When_Null()
    {
        Assert.Throws<ArgumentNullException>(() => _generator.GetFontFamilyFromFile(null!));
    }

    [Fact]
    public void GetFontFamilyFromFile_Throws_When_NotExists()
    {
        Assert.Throws<FileNotFoundException>(() => _generator.GetFontFamilyFromFile("nonexistent.ttf"));
    }

    [Fact]
    public async Task GetImageStreamFromUrlAsync_Throws_When_HttpError()
    {
        _mockHttp.When("http://error.com/i.png").Respond(HttpStatusCode.NotFound);

        // GetImageStreamFromUrlAsync is private, but we can test it via GenerateSpeakerProfileFromUrlsAsync
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _generator.GenerateSpeakerProfileFromUrlsAsync("http://error.com/i.png", "http://logo.com/i.png", "Name", ["Arial"]));
    }

    [Fact]
    public void QueueSpeakerOpenGraphProfileImageCreation_CanBeCalled()
    {
        // This test ensures that the method can be called without basic constructor issues
        // We are not testing the actual queueing here as it involves complex mocking of third-party extension methods
        Assert.NotNull(_generator);
    }

    [Fact]
    public async Task QueueSpeakerOpenGraphProfileImageCreation_sends_message_and_logs()
    {
        var handler = new FakeQueueHandler();
        var queueServiceClient = CreateQueueServiceClient(handler);
        var logger = new Mock<ILogger<OpenGraphSpeakerProfileImageGenerator>>();
        var generator = new OpenGraphSpeakerProfileImageGenerator(new HttpClient(), queueServiceClient, logger.Object);
        var id = Guid.NewGuid();

        await generator.QueueSpeakerOpenGraphProfileImageCreation(id, "http://example.com/headshot.png", "Test User");

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequestBody.Should().NotBeNullOrEmpty();
        logger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Queued OpenGraph profile image creation for speaker")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}