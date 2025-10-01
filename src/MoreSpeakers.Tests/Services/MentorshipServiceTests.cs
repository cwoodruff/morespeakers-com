using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using morespeakers.Models;
using morespeakers.Services;

namespace MoreSpeakers.Tests.Services;

public class MentorshipServiceTests : TestBase
{
    private readonly IMentorshipService _mentorshipService;

    public MentorshipServiceTests()
    {
        _mentorshipService = new MentorshipService(Context);
        SeedMentorshipData();
    }

    private void SeedMentorshipData()
    {
        // Add some mentorships for testing
        var newSpeaker = GetNewSpeaker();
        var experiencedSpeaker = GetExperiencedSpeaker();

        var mentorships = new[]
        {
            new Mentorship
            {
                Id = Guid.NewGuid(),
                MenteeId = newSpeaker.Id,
                MentorId = experiencedSpeaker.Id,
                Status = MentorshipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-5),
                Notes = "Looking for guidance on public speaking"
            },
            new Mentorship
            {
                Id = Guid.NewGuid(),
                MenteeId = newSpeaker.Id,
                MentorId = experiencedSpeaker.Id,
                Status = MentorshipStatus.Active,
                RequestedAt = DateTime.UtcNow.AddDays(-10),
                ResponsedAt = DateTime.UtcNow.AddDays(-8),
                Notes = "Working on first conference talk"
            }
        };

        Context.Mentorship.AddRange(mentorships);
        Context.SaveChanges();
    }

    [Fact]
    public async Task GetMentorshipsForMentorAsync_ShouldReturnMentorshipsForSpecificMentor()
    {
        // Arrange
        var mentor = GetExperiencedSpeaker();

        // Act
        var result = await _mentorshipService.GetMentorshipsForMentorAsync(mentor.Id);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(m => m.MentorId == mentor.Id);
        result.Should().BeInDescendingOrder(m => m.RequestedAt);
    }

    [Fact]
    public async Task GetMentorshipsForNewSpeakerAsync_ShouldReturnMentorshipsForSpecificNewSpeaker()
    {
        // Arrange
        var newSpeaker = GetNewSpeaker();

        // Act
        var result = await _mentorshipService.GetMentorshipsForNewSpeakerAsync(newSpeaker.Id);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(m => m.MenteeId == newSpeaker.Id);
        result.Should().BeInDescendingOrder(m => m.RequestedAt);
    }

    [Fact]
    public async Task GetMentorshipByIdAsync_WithValidId_ShouldReturnMentorship()
    {
        // Arrange
        var existingMentorship = await Context.Mentorship.FirstAsync();

        // Act
        var result = await _mentorshipService.GetMentorshipByIdAsync(existingMentorship.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingMentorship.Id);
        result.Mentee.Should().NotBeNull();
        result.Mentor.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMentorshipByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _mentorshipService.GetMentorshipByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RequestMentorshipAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var newSpeaker = GetNewSpeaker();
        var mentor = GetExperiencedSpeaker();
        var notes = "Need help with presentation skills";

        // First, remove existing mentorships to test fresh request
        var existingMentorships = Context.Mentorship
            .Where(m => m.MenteeId == newSpeaker.Id && m.MentorId == mentor.Id);
        Context.Mentorship.RemoveRange(existingMentorships);
        await Context.SaveChangesAsync();

        // Act
        var result = await _mentorshipService.RequestMentorshipAsync(newSpeaker.Id, mentor.Id, notes);

        // Assert
        result.Should().BeTrue();

        var createdMentorship = await Context.Mentorship
            .FirstOrDefaultAsync(m => m.MenteeId == newSpeaker.Id &&
                                      m.MentorId == mentor.Id &&
                                      m.Status == MentorshipStatus.Pending);

        createdMentorship.Should().NotBeNull();
        createdMentorship!.Notes.Should().Be(notes);
    }

    [Fact]
    public async Task RequestMentorshipAsync_WithExistingActiveRequest_ShouldReturnFalse()
    {
        // Arrange
        var newSpeaker = GetNewSpeaker();
        var mentor = GetExperiencedSpeaker();

        // Act
        var result = await _mentorshipService.RequestMentorshipAsync(newSpeaker.Id, mentor.Id);

        // Assert
        result.Should().BeFalse(); // Should fail because mentorship already exists
    }

    [Fact]
    public async Task AcceptMentorshipAsync_WithPendingMentorship_ShouldReturnTrue()
    {
        // Arrange
        var pendingMentorship = await Context.Mentorship
            .FirstAsync(m => m.Status == MentorshipStatus.Pending);

        // Act
        var result = await _mentorshipService.AcceptMentorshipAsync(pendingMentorship.Id);

        // Assert
        result.Should().BeTrue();

        var updatedMentorship = await Context.Mentorship.FindAsync(pendingMentorship.Id);
        updatedMentorship!.Status.Should().Be(MentorshipStatus.Active);
        updatedMentorship.ResponsedAt.Should().NotBeNull();
        updatedMentorship.ResponsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AcceptMentorshipAsync_WithNonPendingMentorship_ShouldReturnFalse()
    {
        // Arrange
        var activeMentorship = await Context.Mentorship
            .FirstAsync(m => m.Status == MentorshipStatus.Active);

        // Act
        var result = await _mentorshipService.AcceptMentorshipAsync(activeMentorship.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AcceptMentorshipAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _mentorshipService.AcceptMentorshipAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteMentorshipAsync_WithActiveMentorship_ShouldReturnTrue()
    {
        // Arrange
        var activeMentorship = await Context.Mentorship
            .FirstAsync(m => m.Status == MentorshipStatus.Active);
        var completionNotes = "Successfully completed mentorship program";

        // Act
        var result = await _mentorshipService.CompleteMentorshipAsync(activeMentorship.Id, completionNotes);

        // Assert
        result.Should().BeTrue();

        var updatedMentorship = await Context.Mentorship.FindAsync(activeMentorship.Id);
        updatedMentorship!.Status.Should().Be(MentorshipStatus.Completed);
        updatedMentorship.CompletedAt.Should().NotBeNull();
        updatedMentorship.Notes.Should().Contain(completionNotes);
    }

    [Fact]
    public async Task CompleteMentorshipAsync_WithNonActiveMentorship_ShouldReturnFalse()
    {
        // Arrange
        var pendingMentorship = await Context.Mentorship
            .FirstAsync(m => m.Status == MentorshipStatus.Pending);

        // Act
        var result = await _mentorshipService.CompleteMentorshipAsync(pendingMentorship.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelMentorshipAsync_WithValidMentorship_ShouldReturnTrue()
    {
        // Arrange
        var mentorship = await Context.Mentorship
            .FirstAsync(m => m.Status == MentorshipStatus.Pending);
        var cancellationReason = "Schedule conflicts";

        // Act
        var result = await _mentorshipService.CancelMentorshipAsync(mentorship.Id, cancellationReason);

        // Assert
        result.Should().BeTrue();

        var updatedMentorship = await Context.Mentorship.FindAsync(mentorship.Id);
        updatedMentorship!.Status.Should().Be(MentorshipStatus.Cancelled);
        updatedMentorship.Notes.Should().Contain(cancellationReason);
    }

    [Fact]
    public async Task UpdateMentorshipNotesAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var mentorship = await Context.Mentorship.FirstAsync();
        var newNotes = "Updated mentorship notes with progress details";

        // Act
        var result = await _mentorshipService.UpdateMentorshipNotesAsync(mentorship.Id, newNotes);

        // Assert
        result.Should().BeTrue();

        var updatedMentorship = await Context.Mentorship.FindAsync(mentorship.Id);
        updatedMentorship!.Notes.Should().Be(newNotes);
    }

    [Fact]
    public async Task UpdateMentorshipNotesAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var notes = "These notes won't be saved";

        // Act
        var result = await _mentorshipService.UpdateMentorshipNotesAsync(invalidId, notes);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPendingMentorshipsAsync_ShouldReturnOnlyPendingMentorships()
    {
        // Act
        var result = await _mentorshipService.GetPendingMentorshipsAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(m => m.Status == MentorshipStatus.Pending);
        result.Should().BeInAscendingOrder(m => m.RequestedAt);
    }

    [Fact]
    public async Task GetActiveMentorshipsAsync_ShouldReturnOnlyActiveMentorships()
    {
        // Act
        var result = await _mentorshipService.GetActiveMentorshipsAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(m => m.Status == MentorshipStatus.Active);
        result.Should().BeInAscendingOrder(m => m.ResponsedAt);
    }
}