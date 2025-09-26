using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using morespeakers.Services;

namespace MoreSpeakers.Tests.Services;

public class FileUploadServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly FileUploadService _fileUploadService;
    private readonly string _testWebRootPath;
    private readonly string _testUploadsPath;

    public FileUploadServiceTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _testWebRootPath = Path.Combine(Path.GetTempPath(), "FileUploadTests", Guid.NewGuid().ToString());
        _testUploadsPath = Path.Combine(_testWebRootPath, "uploads", "headshots");
        
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testWebRootPath);
        
        _fileUploadService = new FileUploadService(_mockEnvironment.Object);
        
        // Ensure test directories exist
        Directory.CreateDirectory(_testUploadsPath);
    }

    public void Dispose()
    {
        // Cleanup test files
        if (Directory.Exists(_testWebRootPath))
        {
            Directory.Delete(_testWebRootPath, true);
        }
    }

    [Fact]
    public void IsValidImageFile_WithValidJpgFile_ShouldReturnTrue()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.jpg", 1024);

        // Act
        var result = _fileUploadService.IsValidImageFile(mockFile.Object);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("test.jpeg")]
    [InlineData("test.png")]
    [InlineData("test.gif")]
    [InlineData("test.JPG")]
    [InlineData("test.PNG")]
    public void IsValidImageFile_WithValidExtensions_ShouldReturnTrue(string fileName)
    {
        // Arrange
        var mockFile = CreateMockFormFile(fileName, 1024);

        // Act
        var result = _fileUploadService.IsValidImageFile(mockFile.Object);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("test.txt")]
    [InlineData("test.pdf")]
    [InlineData("test.doc")]
    [InlineData("test.exe")]
    [InlineData("test")]
    public void IsValidImageFile_WithInvalidExtensions_ShouldReturnFalse(string fileName)
    {
        // Arrange
        var mockFile = CreateMockFormFile(fileName, 1024);

        // Act
        var result = _fileUploadService.IsValidImageFile(mockFile.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_WithNullFile_ShouldReturnFalse()
    {
        // Act
        var result = _fileUploadService.IsValidImageFile(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_WithEmptyFile_ShouldReturnFalse()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.jpg", 0);

        // Act
        var result = _fileUploadService.IsValidImageFile(mockFile.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_WithTooLargeFile_ShouldReturnFalse()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.jpg", 6 * 1024 * 1024); // 6MB (exceeds 5MB limit)

        // Act
        var result = _fileUploadService.IsValidImageFile(mockFile.Object);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UploadHeadshotAsync_WithValidFile_ShouldReturnFileName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockFile = CreateMockFormFile("profile.jpg", 1024, "fake image content");

        // Act
        var result = await _fileUploadService.UploadHeadshotAsync(mockFile.Object, userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be($"{userId}.jpg");

        // Verify file was created
        var expectedPath = Path.Combine(_testUploadsPath, $"{userId}.jpg");
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public async Task UploadHeadshotAsync_WithInvalidFile_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockFile = CreateMockFormFile("document.txt", 1024);

        // Act
        var result = await _fileUploadService.UploadHeadshotAsync(mockFile.Object, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadHeadshotAsync_WithNullFile_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _fileUploadService.UploadHeadshotAsync(null!, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldCreateDirectoryIfNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockFile = CreateMockFormFile("profile.jpg", 1024, "fake image content");
        
        // Delete the uploads directory to test creation
        if (Directory.Exists(_testUploadsPath))
        {
            Directory.Delete(_testUploadsPath, true);
        }

        // Act
        var result = await _fileUploadService.UploadHeadshotAsync(mockFile.Object, userId);

        // Assert
        result.Should().NotBeNull();
        Directory.Exists(_testUploadsPath).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteHeadshotAsync_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var fileName = "test-headshot.jpg";
        var filePath = Path.Combine(_testUploadsPath, fileName);
        await File.WriteAllTextAsync(filePath, "test content");

        // Act
        var result = await _fileUploadService.DeleteHeadshotAsync(fileName);

        // Assert
        result.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteHeadshotAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var fileName = "non-existent.jpg";

        // Act
        var result = await _fileUploadService.DeleteHeadshotAsync(fileName);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("profile.jpg", "/uploads/headshots/profile.jpg")]
    [InlineData("user123.png", "/uploads/headshots/user123.png")]
    [InlineData("avatar.gif", "/uploads/headshots/avatar.gif")]
    public void GetHeadshotPath_ShouldReturnCorrectPath(string fileName, string expectedPath)
    {
        // Act
        var result = _fileUploadService.GetHeadshotPath(fileName);

        // Assert
        result.Should().Be(expectedPath);
    }

    [Fact]
    public async Task UploadHeadshotAsync_ShouldOverwriteExistingFile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileName = $"{userId}.jpg";
        var filePath = Path.Combine(_testUploadsPath, fileName);
        
        // Create existing file
        await File.WriteAllTextAsync(filePath, "old content");
        
        var mockFile = CreateMockFormFile("new-profile.jpg", 1024, "new image content");

        // Act
        var result = await _fileUploadService.UploadHeadshotAsync(mockFile.Object, userId);

        // Assert
        result.Should().Be(fileName);
        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent.Should().Be("new image content");
    }

    [Fact]
    public async Task UploadHeadshotAsync_WithDifferentExtensions_ShouldPreserveExtension()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockFile = CreateMockFormFile("profile.PNG", 1024, "image content");

        // Act
        var result = await _fileUploadService.UploadHeadshotAsync(mockFile.Object, userId);

        // Assert
        result.Should().Be($"{userId}.png"); // Should normalize to lowercase
        
        var expectedPath = Path.Combine(_testUploadsPath, $"{userId}.png");
        File.Exists(expectedPath).Should().BeTrue();
    }

    private static Mock<IFormFile> CreateMockFormFile(string fileName, long length, string content = "fake content")
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;
        
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));
        
        return mockFile;
    }
}