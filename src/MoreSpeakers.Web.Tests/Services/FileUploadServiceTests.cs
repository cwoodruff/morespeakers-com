using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FileUploadService"/>.
/// File I/O is exercised against a real temporary directory that is cleaned up in Dispose.
/// </summary>
public sealed class FileUploadServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _environmentMock = new();
    private readonly Mock<ILogger<FileUploadService>> _loggerMock = new();
    private readonly FileUploadService _sut;
    private readonly string _tempWebRoot;

    public FileUploadServiceTests()
    {
        _tempWebRoot = Path.Combine(Path.GetTempPath(), $"morespeakers-fileupload-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempWebRoot);
        _environmentMock.Setup(e => e.WebRootPath).Returns(_tempWebRoot);
        _sut = new FileUploadService(_environmentMock.Object, _loggerMock.Object);
    }

    // ───────────────────────────────────────────────────────────────────────────
    // Helpers
    // ───────────────────────────────────────────────────────────────────────────

    private static Mock<IFormFile> CreateFormFileMock(string fileName, long length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(length);
        // CopyToAsync is called with the target stream; we return completed so the
        // FileStream is still opened (creating the file) but nothing is written.
        fileMock
            .Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return fileMock;
    }

    private string EnsureUploadsDir()
    {
        var dir = Path.Combine(_tempWebRoot, "uploads", "headshots");
        Directory.CreateDirectory(dir);
        return dir;
    }

    // ───────────────────────────────────────────────────────────────────────────
    // IsValidImageFile
    // ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsValidImageFile_ShouldReturnFalse_WhenFileLengthIsZero()
    {
        // Arrange
        var file = CreateFormFileMock("headshot.jpg", 0);

        // Act
        var isValid = _sut.IsValidImageFile(file.Object);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_ShouldReturnFalse_WhenFileSizeExceedsMaximumAllowed()
    {
        // Arrange – 6 MB exceeds the 5 MB limit
        var file = CreateFormFileMock("headshot.jpg", 6 * 1024 * 1024);

        // Act
        var isValid = _sut.IsValidImageFile(file.Object);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_ShouldReturnTrue_WhenFileSizeIsExactlyAtMaximum()
    {
        // Arrange – exactly 5 MB is NOT > MaxFileSize so it should be valid
        var file = CreateFormFileMock("headshot.jpg", 5 * 1024 * 1024);

        // Act
        var isValid = _sut.IsValidImageFile(file.Object);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("headshot.jpg")]
    [InlineData("headshot.jpeg")]
    [InlineData("headshot.png")]
    [InlineData("headshot.gif")]
    public void IsValidImageFile_ShouldReturnTrue_WhenExtensionIsAllowed(string fileName)
    {
        // Arrange
        var file = CreateFormFileMock(fileName, 1024);

        // Act
        var isValid = _sut.IsValidImageFile(file.Object);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("HEADSHOT.JPG")]
    [InlineData("HEADSHOT.PNG")]
    [InlineData("Headshot.Jpeg")]
    [InlineData("avatar.GIF")]
    public void IsValidImageFile_ShouldReturnTrue_WhenExtensionIsUpperCase(string fileName)
    {
        // Arrange – implementation normalises via ToLowerInvariant
        var file = CreateFormFileMock(fileName, 1024);

        // Act
        var isValid = _sut.IsValidImageFile(file.Object);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("spreadsheet.xlsx")]
    [InlineData("archive.zip")]
    [InlineData("script.js")]
    [InlineData("noextension")]
    public void IsValidImageFile_ShouldReturnFalse_WhenExtensionIsNotAllowed(string fileName)
    {
        // Arrange
        var file = CreateFormFileMock(fileName, 1024);

        // Act
        var isValid = _sut.IsValidImageFile(file.Object);

        // Assert
        isValid.Should().BeFalse();
    }

    // ───────────────────────────────────────────────────────────────────────────
    // GetHeadshotPath
    // ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetHeadshotPath_ShouldReturnCorrectRelativePath_WhenFileNameIsProvided()
    {
        // Arrange
        const string fileName = "johndoe.jpg";

        // Act
        var path = _sut.GetHeadshotPath(fileName);

        // Assert
        path.Should().Be($"/uploads/headshots/{fileName}");
    }

    [Fact]
    public void GetHeadshotPath_ShouldEmbedGuidFileName_WhenFileNameContainsGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var fileName = $"{guid}.png";

        // Act
        var path = _sut.GetHeadshotPath(fileName);

        // Assert
        path.Should().Be($"/uploads/headshots/{fileName}");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // UploadHeadshotAsync — invalid-file early-exit paths (no I/O)
    // ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UploadHeadshotAsync_ShouldReturnNull_WhenFileIsEmpty()
    {
        // Arrange
        var file = CreateFormFileMock("headshot.jpg", 0);
        var userId = Guid.NewGuid();

        // Act
        var result = await _sut.UploadHeadshotAsync(file.Object, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldReturnNull_WhenFileExtensionIsNotAllowed()
    {
        // Arrange
        var file = CreateFormFileMock("resume.pdf", 1024);
        var userId = Guid.NewGuid();

        // Act
        var result = await _sut.UploadHeadshotAsync(file.Object, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldReturnNull_WhenFileSizeExceedsLimit()
    {
        // Arrange – 10 MB is well over the 5 MB cap
        var file = CreateFormFileMock("headshot.jpg", 10 * 1024 * 1024);
        var userId = Guid.NewGuid();

        // Act
        var result = await _sut.UploadHeadshotAsync(file.Object, userId);

        // Assert
        result.Should().BeNull();
    }

    // ───────────────────────────────────────────────────────────────────────────
    // UploadHeadshotAsync — happy path (real temp directory)
    // ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UploadHeadshotAsync_ShouldReturnFileName_WhenFileIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var file = CreateFormFileMock("photo.jpg", 1024);

        // Act
        var result = await _sut.UploadHeadshotAsync(file.Object, userId);

        // Assert – returned name is "<userId>.<original-extension>"
        result.Should().Be($"{userId}.jpg");
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldPreserveOriginalExtension_WhenFileIsPng()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var file = CreateFormFileMock("avatar.png", 2048);

        // Act
        var result = await _sut.UploadHeadshotAsync(file.Object, userId);

        // Assert
        result.Should().Be($"{userId}.png");
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldCreateUploadsDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var file = CreateFormFileMock("photo.png", 1024);
        var expectedDir = Path.Combine(_tempWebRoot, "uploads", "headshots");

        // Act
        await _sut.UploadHeadshotAsync(file.Object, userId);

        // Assert – directory was created as a side-effect
        Directory.Exists(expectedDir).Should().BeTrue();
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldPlaceFileInHeadshotsDirectory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var file = CreateFormFileMock("photo.jpg", 1024);

        // Act
        var fileName = await _sut.UploadHeadshotAsync(file.Object, userId);

        // Assert – the file was physically created on disk
        var expectedPath = Path.Combine(_tempWebRoot, "uploads", "headshots", fileName!);
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldReturnNull_WhenExceptionOccursDuringUpload()
    {
        // Arrange – mock throws when the stream copy is attempted
        var userId = Guid.NewGuid();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("photo.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock
            .Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Simulated disk error"));

        // Act
        var result = await _sut.UploadHeadshotAsync(fileMock.Object, userId);

        // Assert
        result.Should().BeNull();
    }

    // ───────────────────────────────────────────────────────────────────────────
    // DeleteHeadshotAsync
    // ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DeleteHeadshotAsync_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        // Arrange
        const string fileName = "ghost-speaker.jpg";

        // Act
        var result = _sut.DeleteHeadshotAsync(fileName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DeleteHeadshotAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange – plant a file in the expected headshots location
        var uploadsDir = EnsureUploadsDir();
        var fileName = $"{Guid.NewGuid()}.jpg";
        File.WriteAllText(Path.Combine(uploadsDir, fileName), "fake-image-bytes");

        // Act
        var result = _sut.DeleteHeadshotAsync(fileName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DeleteHeadshotAsync_ShouldRemoveFileFromDisk_WhenFileExists()
    {
        // Arrange
        var uploadsDir = EnsureUploadsDir();
        var fileName = $"{Guid.NewGuid()}.png";
        var filePath = Path.Combine(uploadsDir, fileName);
        File.WriteAllText(filePath, "fake-image-bytes");

        // Act
        _sut.DeleteHeadshotAsync(fileName);

        // Assert – the file is no longer on disk
        File.Exists(filePath).Should().BeFalse();
    }

    // ───────────────────────────────────────────────────────────────────────────
    // Cleanup
    // ───────────────────────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (Directory.Exists(_tempWebRoot))
        {
            try { Directory.Delete(_tempWebRoot, recursive: true); }
            catch { /* best-effort */ }
        }
    }
}
