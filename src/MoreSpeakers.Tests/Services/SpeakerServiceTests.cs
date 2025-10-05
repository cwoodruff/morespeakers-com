using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Tests.Services;

public class SpeakerServiceTests : TestBase
{
    private readonly SpeakerService _speakerService;

    public SpeakerServiceTests()
    {
        _speakerService = new SpeakerService(Context);
    }

    [Fact]
    public async Task GetNewSpeakersAsync_ShouldReturnOnlyNewSpeakers()
    {
        // Act
        var result = await _speakerService.GetNewSpeakersAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.SpeakerType.Name == "NewSpeaker");
        result.Should().BeInAscendingOrder(s => s.FirstName);
    }

    [Fact]
    public async Task GetExperiencedSpeakersAsync_ShouldReturnOnlyExperiencedSpeakers()
    {
        // Act
        var result = await _speakerService.GetExperiencedSpeakersAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.SpeakerType.Name == "ExperiencedSpeaker");
        result.Should().BeInAscendingOrder(s => s.FirstName);
    }

    [Fact]
    public async Task GetSpeakerByIdAsync_WithValidId_ShouldReturnSpeaker()
    {
        // Arrange
        var expectedSpeaker = GetNewSpeaker();

        // Act
        var result = await _speakerService.GetSpeakerByIdAsync(expectedSpeaker.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedSpeaker.Id);
        result.FirstName.Should().Be(expectedSpeaker.FirstName);
        result.SpeakerType.Should().NotBeNull();
        result.UserExpertise.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSpeakerByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _speakerService.GetSpeakerByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchSpeakersAsync_WithFirstName_ShouldReturnMatchingSpeakers()
    {
        // Arrange
        var searchTerm = "John";

        // Act
        var result = await _speakerService.SearchSpeakersAsync(searchTerm);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.FirstName.Contains(searchTerm));
    }

    [Fact]
    public async Task SearchSpeakersAsync_WithBioContent_ShouldReturnMatchingSpeakers()
    {
        // Arrange
        var searchTerm = "guidance";

        // Act
        var result = await _speakerService.SearchSpeakersAsync(searchTerm);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.Bio.Contains(searchTerm));
    }

    [Fact]
    public async Task SearchSpeakersAsync_WithExpertiseArea_ShouldReturnMatchingSpeakers()
    {
        // Arrange
        var searchTerm = "C#";

        // Act
        var result = await _speakerService.SearchSpeakersAsync(searchTerm);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.UserExpertise.Any(ue => ue.Expertise.Name.Contains(searchTerm)));
    }

    [Fact]
    public async Task SearchSpeakersAsync_WithSpeakerTypeFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var searchTerm = "";
        var speakerTypeId = 1; // NewSpeaker

        // Act
        var result = await _speakerService.SearchSpeakersAsync(searchTerm, speakerTypeId);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.SpeakerTypeId == speakerTypeId);
    }

    [Fact]
    public async Task GetSpeakersByExpertiseAsync_ShouldReturnSpeakersWithSpecifiedExpertise()
    {
        // Arrange
        var expertiseId = 1; // C#

        // Act
        var result = await _speakerService.GetSpeakersByExpertiseAsync(expertiseId);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId));
    }

    [Fact]
    public async Task UpdateSpeakerProfileAsync_WithValidUser_ShouldReturnTrue()
    {
        // Arrange
        var speaker = GetNewSpeaker();
        speaker.Bio = "Updated bio content";

        // Act
        var result = await _speakerService.UpdateSpeakerProfileAsync(speaker);

        // Assert
        result.Should().BeTrue();

        var updatedSpeaker = await Context.Users.FindAsync(speaker.Id);
        updatedSpeaker!.Bio.Should().Be("Updated bio content");
    }

    [Fact]
    public async Task AddSocialMediaLinkAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var speaker = GetNewSpeaker();
        var platform = "GitHub";
        var url = "https://github.com/johndoe";

        // Act
        var result = await _speakerService.AddSocialMediaLinkAsync(speaker.Id, platform, url);

        // Assert
        result.Should().BeTrue();

        var socialMedia = await Context.SocialMedia
            .FirstOrDefaultAsync(sm => sm.UserId == speaker.Id && sm.Platform == platform);

        socialMedia.Should().NotBeNull();
        socialMedia!.Url.Should().Be(url);
    }

    [Fact]
    public async Task RemoveSocialMediaLinkAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var socialMedia = await Context.SocialMedia.FirstAsync();
        var socialMediaId = socialMedia.Id;

        // Act
        var result = await _speakerService.RemoveSocialMediaLinkAsync(socialMediaId);

        // Assert
        result.Should().BeTrue();

        var deletedSocialMedia = await Context.SocialMedia.FindAsync(socialMediaId);
        deletedSocialMedia.Should().BeNull();
    }

    [Fact]
    public async Task RemoveSocialMediaLinkAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = 99999;

        // Act
        var result = await _speakerService.RemoveSocialMediaLinkAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddExpertiseToUserAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var speaker = GetNewSpeaker();
        var expertiseId = 5; // React - not currently assigned to this user

        // Act
        var result = await _speakerService.AddExpertiseToUserAsync(speaker.Id, expertiseId);

        // Assert
        result.Should().BeTrue();

        var userExpertise = await Context.UserExpertise
            .FirstOrDefaultAsync(ue => ue.UserId == speaker.Id && ue.ExpertiseId == expertiseId);

        userExpertise.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveExpertiseFromUserAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var speaker = GetNewSpeaker();
        var userExpertise = await Context.UserExpertise
            .FirstAsync(ue => ue.UserId == speaker.Id);
        var expertiseId = userExpertise.ExpertiseId;

        // Act
        var result = await _speakerService.RemoveExpertiseFromUserAsync(speaker.Id, expertiseId);

        // Assert
        result.Should().BeTrue();

        var deletedUserExpertise = await Context.UserExpertise
            .FirstOrDefaultAsync(ue => ue.UserId == speaker.Id && ue.ExpertiseId == expertiseId);

        deletedUserExpertise.Should().BeNull();
    }

    [Fact]
    public async Task RemoveExpertiseFromUserAsync_WithNonExistentRelationship_ShouldReturnFalse()
    {
        // Arrange
        var speaker = GetNewSpeaker();
        var expertiseId = 99999; // Non-existent expertise

        // Act
        var result = await _speakerService.RemoveExpertiseFromUserAsync(speaker.Id, expertiseId);

        // Assert
        result.Should().BeFalse();
    }
}