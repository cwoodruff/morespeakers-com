using FluentAssertions;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers.Providers;

namespace MoreSpeakers.Managers.Tests.Providers;

public class DatabaseChangeTokenTests
{
    [Fact]
    public void Constructor_ShouldSetEmailTemplate()
    {
        // Arrange
        var template = new EmailTemplate { Location = "test" };

        // Act
        var token = new DatabaseChangeToken(template);

        // Assert
        token.HasChanged.Should().BeFalse();
        token.ActiveChangeCallbacks.Should().BeFalse();
    }

    [Fact]
    public void HasChanged_ShouldReturnFalse_WhenLastRequestedIsNull()
    {
        // Arrange
        var template = new EmailTemplate 
        { 
            LastModified = DateTime.UtcNow,
            LastRequested = null 
        };
        var token = new DatabaseChangeToken(template);

        // Act & Assert
        token.HasChanged.Should().BeFalse();
    }

    [Fact]
    public void HasChanged_ShouldReturnTrue_WhenLastModifiedIsGreaterThanLastRequested()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var template = new EmailTemplate
        {
            LastModified = now.AddMinutes(1),
            LastRequested = now
        };
        var token = new DatabaseChangeToken(template);

        // Act & Assert
        token.HasChanged.Should().BeTrue();
    }

    [Fact]
    public void HasChanged_ShouldReturnFalse_WhenLastModifiedIsLessThanLastRequested()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var template = new EmailTemplate
        {
            LastModified = now.AddMinutes(-1),
            LastRequested = now
        };
        var token = new DatabaseChangeToken(template);

        // Act & Assert
        token.HasChanged.Should().BeFalse();
    }

    [Fact]
    public void HasChanged_ShouldReturnFalse_WhenLastModifiedIsEqualLastRequested()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var template = new EmailTemplate
        {
            LastModified = now,
            LastRequested = now
        };
        var token = new DatabaseChangeToken(template);

        // Act & Assert
        token.HasChanged.Should().BeFalse();
    }

    [Fact]
    public void RegisterChangeCallback_ShouldReturnDisposable()
    {
        // Arrange
        var template = new EmailTemplate { Location = "test" };
        var token = new DatabaseChangeToken(template);

        // Act
        var disposable = token.RegisterChangeCallback(_ => { }, null);

        // Assert
        disposable.Should().NotBeNull();
    }
}