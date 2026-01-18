using FluentAssertions;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers.Providers;
using System.Text;

namespace MoreSpeakers.Managers.Tests.Providers;

public class DatabaseFileInfoTests
{
    [Fact]
    public void Constructor_WithTemplate_ShouldPopulateProperties()
    {
        // Arrange
        var content = "Hello World";
        var lastModified = DateTime.UtcNow;
        var template = new EmailTemplate 
        { 
            Location = "test.cshtml", 
            Content = content,
            LastModified = lastModified
        };

        // Act
        var fileInfo = new DatabaseFileInfo(template, "test.cshtml");

        // Assert
        fileInfo.Exists.Should().BeTrue();
        fileInfo.Name.Should().Be("test.cshtml");
        fileInfo.Length.Should().Be(Encoding.UTF8.GetByteCount(content));
        fileInfo.LastModified.Should().Be(lastModified);
        fileInfo.IsDirectory.Should().BeFalse();
        fileInfo.PhysicalPath.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithoutTemplate_ShouldSetExistsToFalse()
    {
        // Act
        var fileInfo = new DatabaseFileInfo(null, "missing.cshtml");

        // Assert
        fileInfo.Exists.Should().BeFalse();
        fileInfo.Length.Should().Be(-1);
    }

    [Fact]
    public void CreateReadStream_WithTemplate_ShouldReturnContent()
    {
        // Arrange
        var content = "Hello World";
        var template = new EmailTemplate { Content = content };
        var fileInfo = new DatabaseFileInfo(template, "test.cshtml");

        // Act
        using var stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void CreateReadStream_WithoutTemplate_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var fileInfo = new DatabaseFileInfo(null, "missing.cshtml");

        // Act
        var action = () => fileInfo.CreateReadStream();

        // Assert
        action.Should().Throw<FileNotFoundException>();
    }
}