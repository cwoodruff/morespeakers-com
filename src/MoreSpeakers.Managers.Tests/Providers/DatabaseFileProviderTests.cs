using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers.Providers;

namespace MoreSpeakers.Managers.Tests.Providers;

public class DatabaseFileProviderTests
{
    private readonly Mock<IEmailTemplateManager> _managerMock = new();
    private readonly DatabaseFileProvider _provider;

    public DatabaseFileProviderTests()
    {
        _provider = new DatabaseFileProvider(_managerMock.Object);
    }

    [Fact]
    public void GetFileInfo_ExistingFile_ShouldReturnFileInfoWithData()
    {
        // Arrange
        var path = "Templates/Welcome.cshtml";
        var template = new EmailTemplate { Location = path, Content = "Welcome!" };
        _managerMock.Setup(m => m.GetByLocationAsync(path)).ReturnsAsync(template);

        // Act
        var result = _provider.GetFileInfo(path);

        // Assert
        result.Should().NotBeNull();
        result.Exists.Should().BeTrue();
        result.Name.Should().Be(path);
        _managerMock.Verify(m => m.GetByLocationAsync(path), Times.Once);
    }

    [Fact]
    public void GetFileInfo_NonExistingFile_ShouldReturnNotFoundFileInfo()
    {
        // Arrange
        var path = "Missing.cshtml";
        _managerMock.Setup(m => m.GetByLocationAsync(path)).ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = _provider.GetFileInfo(path);

        // Assert
        result.Should().NotBeNull();
        result.Exists.Should().BeFalse();
    }

    [Fact]
    public void GetDirectoryContents_ShouldReturnNotFound()
    {
        // Act
        var result = _provider.GetDirectoryContents("any");

        // Assert
        result.Exists.Should().BeFalse();
    }

    [Fact]
    public void Watch_ExistingFile_ShouldReturnDatabaseChangeToken()
    {
        // Arrange
        var filter = "Templates/Welcome.cshtml";
        var template = new EmailTemplate { Location = filter };
        _managerMock.Setup(m => m.GetByLocationAsync(filter)).ReturnsAsync(template);

        // Act
        var result = _provider.Watch(filter);

        // Assert
        result.Should().BeOfType<DatabaseChangeToken>();
    }

    [Fact]
    public void Watch_NonExistingFile_ShouldReturnNullChangeToken()
    {
        // Arrange
        var filter = "Missing.cshtml";
        _managerMock.Setup(m => m.GetByLocationAsync(filter)).ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = _provider.Watch(filter);

        // Assert
        result.Should().BeSameAs(NullChangeToken.Singleton);
    }
}