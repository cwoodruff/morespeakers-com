using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers;

namespace MoreSpeakers.Managers.Tests;

public class MentoringManagerTests
{
    private readonly Mock<IMentoringDataStore> _dataStoreMock = new();
    private readonly Mock<ILogger<MentoringManager>> _loggerMock = new();

    private MentoringManager CreateSut() => new(_dataStoreMock.Object, _loggerMock.Object);

    [Fact]
    public async Task GetAsync_should_delegate()
    {
        var id = Guid.NewGuid();
        var expected = new Mentorship { Id = id };
        _dataStoreMock.Setup(d => d.GetAsync(id)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAsync(id);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_by_id_should_delegate()
    {
        var id = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(id);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_should_delegate()
    {
        var entity = new Mentorship { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.SaveAsync(entity)).ReturnsAsync(entity);
        var sut = CreateSut();

        var result = await sut.SaveAsync(entity);

        result.Should().BeSameAs(entity);
        _dataStoreMock.Verify(d => d.SaveAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_should_delegate()
    {
        var expected = new List<Mentorship> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetAllAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_by_entity_should_delegate()
    {
        var entity = new Mentorship { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.DeleteAsync(entity)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(entity);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetSharedExpertisesAsync_should_delegate()
    {
        var mentor = new User { Id = Guid.NewGuid() };
        var mentee = new User { Id = Guid.NewGuid() };
        var expected = new List<Expertise> { new() { Id = 1 } };
        _dataStoreMock.Setup(d => d.GetSharedExpertisesAsync(mentor, mentee)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetSharedExpertisesAsync(mentor, mentee);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetSharedExpertisesAsync(mentor, mentee), Times.Once);
    }

    [Fact]
    public async Task DoesMentorshipRequestsExistsAsync_should_delegate()
    {
        var mentor = new User { Id = Guid.NewGuid() };
        var mentee = new User { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.DoesMentorshipRequestsExistsAsync(mentor, mentee)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DoesMentorshipRequestsExistsAsync(mentor, mentee);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DoesMentorshipRequestsExistsAsync(mentor, mentee), Times.Once);
    }

    [Fact]
    public async Task CreateMentorshipRequestAsync_should_delegate()
    {
        var mentorship = new Mentorship { Id = Guid.NewGuid() };
        var expertiseIds = new List<int> { 1, 2 };
        _dataStoreMock.Setup(d => d.CreateMentorshipRequestAsync(mentorship, expertiseIds)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.CreateMentorshipRequestAsync(mentorship, expertiseIds);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.CreateMentorshipRequestAsync(mentorship, expertiseIds), Times.Once);
    }

    [Fact]
    public async Task RespondToRequestAsync_should_delegate()
    {
        var mentorshipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expected = new Mentorship { Id = mentorshipId };
        _dataStoreMock.Setup(d => d.RespondToRequestAsync(mentorshipId, userId, true, "ok")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.RespondToRequestAsync(mentorshipId, userId, true, "ok");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.RespondToRequestAsync(mentorshipId, userId, true, "ok"), Times.Once);
    }

    [Fact]
    public async Task GetActiveMentorshipsForUserAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        var expected = new List<Mentorship> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetActiveMentorshipsForUserAsync(userId)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetActiveMentorshipsForUserAsync(userId);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetActiveMentorshipsForUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetNumberOfMentorshipsPending_should_delegate()
    {
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.GetNumberOfMentorshipsPending(userId)).ReturnsAsync((2, 3));
        var sut = CreateSut();

        var result = await sut.GetNumberOfMentorshipsPending(userId);

        result.Should().Be((2, 3));
        _dataStoreMock.Verify(d => d.GetNumberOfMentorshipsPending(userId), Times.Once);
    }

    [Fact]
    public async Task GetIncomingMentorshipRequests_should_delegate()
    {
        var userId = Guid.NewGuid();
        var expected = new List<Mentorship> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetIncomingMentorshipRequests(userId)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetIncomingMentorshipRequests(userId);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetIncomingMentorshipRequests(userId), Times.Once);
    }

    [Fact]
    public async Task GetOutgoingMentorshipRequests_should_delegate()
    {
        var userId = Guid.NewGuid();
        var expected = new List<Mentorship> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetOutgoingMentorshipRequests(userId)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetOutgoingMentorshipRequests(userId);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetOutgoingMentorshipRequests(userId), Times.Once);
    }

    [Fact]
    public async Task CancelMentorshipRequestAsync_should_delegate()
    {
        var mentorshipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.CancelMentorshipRequestAsync(mentorshipId, userId)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.CancelMentorshipRequestAsync(mentorshipId, userId);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.CancelMentorshipRequestAsync(mentorshipId, userId), Times.Once);
    }

    [Fact]
    public async Task CompleteMentorshipRequestAsync_should_delegate()
    {
        var mentorshipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.CompleteMentorshipRequestAsync(mentorshipId, userId)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.CompleteMentorshipRequestAsync(mentorshipId, userId);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.CompleteMentorshipRequestAsync(mentorshipId, userId), Times.Once);
    }

    [Fact]
    public async Task GetMentorsExceptForUserAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        var expected = new List<User> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetMentorsExceptForUserAsync(userId, MentorshipType.NewToExperienced, new List<string> { "AI" }, true)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetMentorsExceptForUserAsync(userId, MentorshipType.NewToExperienced, new List<string> { "AI" }, true);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetMentorsExceptForUserAsync(userId, MentorshipType.NewToExperienced, It.IsAny<List<string>>(), true), Times.Once);
    }

    [Fact]
    public async Task GetMentorAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        var expected = new User { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.GetMentorAsync(userId)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetMentorAsync(userId);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetMentorAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CanRequestMentorshipAsync_should_delegate()
    {
        var menteeId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.CanRequestMentorshipAsync(menteeId, mentorId)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.CanRequestMentorshipAsync(menteeId, mentorId);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.CanRequestMentorshipAsync(menteeId, mentorId), Times.Once);
    }

    [Fact]
    public async Task RequestMentorshipWithDetailsAsync_should_delegate()
    {
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var expected = new Mentorship { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.RequestMentorshipWithDetailsAsync(requesterId, targetId, MentorshipType.NewToExperienced, "msg", It.IsAny<List<int>?>(), "weekly")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.RequestMentorshipWithDetailsAsync(requesterId, targetId, MentorshipType.NewToExperienced, "msg", new List<int> { 1, 2 }, "weekly");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.RequestMentorshipWithDetailsAsync(requesterId, targetId, MentorshipType.NewToExperienced, "msg", It.IsAny<List<int>?>(), "weekly"), Times.Once);
    }

    [Fact]
    public async Task GetMentorshipWithRelationships_should_delegate()
    {
        var mentorshipId = Guid.NewGuid();
        var expected = new Mentorship { Id = mentorshipId };
        _dataStoreMock.Setup(d => d.GetMentorshipWithRelationships(mentorshipId)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetMentorshipWithRelationships(mentorshipId);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetMentorshipWithRelationships(mentorshipId), Times.Once);
    }
}
