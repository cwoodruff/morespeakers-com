using FluentAssertions;

using MoreSpeakers.Domain.Models;

using DataMentorship = MoreSpeakers.Data.Models.Mentorship;

namespace MoreSpeakers.Data.Tests;

public class MentoringDataStoreTests : DataStoreTestBase
{
    private MentoringDataStore CreateMentoringStore() => new(Context, Mapper, CreateLogger<MentoringDataStore>().Object);

    [Fact]
    public async Task GetAsync_WhenMentorshipExists_ShouldReturnSuccess()
    {
        var speakerType = await AddSpeakerTypeAsync();
        var mentor = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var mentee = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        
        var mentorship = new DataMentorship
        {
            Id = Guid.NewGuid(),
            MentorId = mentor.Id,
            MenteeId = mentee.Id,
            Status = Models.MentorshipStatus.Pending,
            Type = Models.MentorshipType.NewToExperienced,
            RequestedAt = DateTime.UtcNow
        };
        Context.Mentorship.Add(mentorship);
        await Context.SaveChangesAsync(CancellationToken);

        var sut = CreateMentoringStore();

        var result = await sut.GetAsync(mentorship.Id);

        result.ShouldSucceed();
        result.Value.Id.Should().Be(mentorship.Id);
        result.Value.MentorId.Should().Be(mentor.Id);
        result.Value.MenteeId.Should().Be(mentee.Id);
    }

    [Fact]
    public async Task GetAsync_WhenMentorshipDoesNotExist_ShouldReturnFailure()
    {
        var sut = CreateMentoringStore();

        var result = await sut.GetAsync(Guid.NewGuid());

        result.ShouldFail("mentorship.not-found");
    }

    [Fact]
    public async Task SaveAsync_WhenMentorshipIsNew_ShouldPersistAndReturnSuccess()
    {
        var speakerType = await AddSpeakerTypeAsync();
        var mentor = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var mentee = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var sut = CreateMentoringStore();

        var result = await sut.SaveAsync(new Domain.Models.Mentorship
        {
            Id = Guid.NewGuid(),
            MentorId = mentor.Id,
            MenteeId = mentee.Id,
            Status = MentorshipStatus.Pending,
            Type = MentorshipType.NewToExperienced,
            RequestedAt = DateTime.UtcNow
        });

        result.ShouldSucceed();
        result.Value.Id.Should().NotBeEmpty();

        var persisted = await Context.Mentorship.FindAsync([result.Value.Id], CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.MentorId.Should().Be(mentor.Id);
        persisted.MenteeId.Should().Be(mentee.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenMentorshipExists_ShouldRemoveAndReturnSuccess()
    {
        var speakerType = await AddSpeakerTypeAsync();
        var mentor = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var mentee = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        
        var mentorship = new DataMentorship
        {
            Id = Guid.NewGuid(),
            MentorId = mentor.Id,
            MenteeId = mentee.Id,
            Status = Models.MentorshipStatus.Pending,
            Type = Models.MentorshipType.NewToExperienced,
            RequestedAt = DateTime.UtcNow
        };
        Context.Mentorship.Add(mentorship);
        await Context.SaveChangesAsync(CancellationToken);
        Context.ChangeTracker.Clear();

        var sut = CreateMentoringStore();

        var result = await sut.DeleteAsync(mentorship.Id);

        result.ShouldSucceed();

        var persisted = await Context.Mentorship.FindAsync([mentorship.Id], CancellationToken);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenMentorshipDoesNotExist_ShouldReturnFailure()
    {
        var sut = CreateMentoringStore();

        var result = await sut.DeleteAsync(Guid.NewGuid());

        result.ShouldFail("mentorship.delete.not-found");
    }

    [Fact]
    public async Task GetActiveMentorshipsForUserAsync_WhenUserHasActiveMentorships_ShouldReturnList()
    {
        var speakerType = await AddSpeakerTypeAsync();
        var mentor = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var mentee = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        
        var mentorship = new DataMentorship
        {
            Id = Guid.NewGuid(),
            MentorId = mentor.Id,
            MenteeId = mentee.Id,
            Status = Models.MentorshipStatus.Active,
            Type = Models.MentorshipType.NewToExperienced,
            RequestedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };
        Context.Mentorship.Add(mentorship);
        await Context.SaveChangesAsync(CancellationToken);

        var sut = CreateMentoringStore();

        var result = await sut.GetActiveMentorshipsForUserAsync(mentor.Id);

        result.ShouldSucceed();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(mentorship.Id);
    }

    [Fact]
    public async Task GetActiveMentorshipsForUserAsync_WhenUserHasNoActiveMentorships_ShouldReturnEmptyList()
    {
        var speakerType = await AddSpeakerTypeAsync();
        var user = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var sut = CreateMentoringStore();

        var result = await sut.GetActiveMentorshipsForUserAsync(user.Id);

        result.ShouldSucceed();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task CanRequestMentorshipAsync_WhenNoPendingOrActiveMentorship_ShouldReturnTrue()
    {
        var speakerType = await AddSpeakerTypeAsync();
        var mentor = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var mentee = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var sut = CreateMentoringStore();

        var result = await sut.CanRequestMentorshipAsync(mentee.Id, mentor.Id);

        result.ShouldSucceed();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CanRequestMentorshipAsync_WhenPendingMentorshipExists_ShouldReturnFalse()
    {
        var speakerType = await AddSpeakerTypeAsync();
        var mentor = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        var mentee = await AddUserAsync(u => u.SpeakerTypeId = speakerType.Id);
        
        var mentorship = new DataMentorship
        {
            Id = Guid.NewGuid(),
            MentorId = mentor.Id,
            MenteeId = mentee.Id,
            Status = Models.MentorshipStatus.Pending,
            Type = Models.MentorshipType.NewToExperienced,
            RequestedAt = DateTime.UtcNow
        };
        Context.Mentorship.Add(mentorship);
        await Context.SaveChangesAsync(CancellationToken);

        var sut = CreateMentoringStore();

        var result = await sut.CanRequestMentorshipAsync(mentee.Id, mentor.Id);

        result.ShouldSucceed();
        result.Value.Should().BeFalse();
    }
}
