using FluentAssertions;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Tests.Services;

public class OpenGraphServiceTests
{
    private readonly OpenGraphService _service;
    private readonly Mock<ISettings> _settingsMock;
    private const string BlobUrl = "https://blob.example.com/";

    public OpenGraphServiceTests()
    {
        _settingsMock = new Mock<ISettings>();
        _settingsMock.Setup(s => s.OpenGraph).Returns(new OpenGraphSettings { SpeakerCardBlobUrl = BlobUrl });
        _service = new OpenGraphService(_settingsMock.Object);
    }

    [Fact]
    public void GenerateUserMetadata_ShouldReturnCorrectMetadata_WhenUserHasAllFields()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = "Experienced speaker with a passion for C#.",
            HeadshotUrl = "https://example.com/headshot.jpg"
        };
        var profileUrl = "https://morespeakers.com/speakers/john-doe";

        // Act
        var result = _service.GenerateUserMetadata(user, profileUrl);

        // Assert
        result["og:title"].Should().Be("John Doe's MoreSpeakers.com Profile");
        result["og:type"].Should().Be("profile");
        result["og:url"].Should().Be(profileUrl);
        result["og:description"].Should().Be(user.Bio);
        result["og:profile:first_name"].Should().Be("John");
        result["og:profile:last_name"].Should().Be("Doe");
        result["og:image"].Should().Be($"{BlobUrl}{user.Id}.png");
    }

    [Fact]
    public void GenerateUserMetadata_ShouldNotIncludeImage_WhenHeadshotUrlIsEmpty()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Jane",
            LastName = "Smith",
            Bio = "New speaker.",
            HeadshotUrl = null
        };
        var profileUrl = "https://morespeakers.com/speakers/jane-smith";

        // Act
        var result = _service.GenerateUserMetadata(user, profileUrl);

        // Assert
        result.Should().NotContainKey("og:image");
    }

    [Fact]
    public void GenerateUserMetadata_ShouldTruncateDescription_WhenBioIsTooLong()
    {
        // Arrange
        var longBio = new string('a', 400);
        var user = new User
        {
            FirstName = "Alice",
            LastName = "Wonderland",
            Bio = longBio
        };
        var profileUrl = "https://morespeakers.com/speakers/alice-wonderland";

        // Act
        var result = _service.GenerateUserMetadata(user, profileUrl);

        // Assert
        result["og:description"].Should().HaveLength(303); // 300 + "..."
        result["og:description"].Should().EndWith("...");
    }
}