using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers.Tests;

public class UserManagerTests
{
    private readonly Mock<IUserDataStore> _dataStoreMock = new();

    private UserManager CreateSut() => new(_dataStoreMock.Object);

    // ---------- Identity wrapper methods ----------
    [Fact]
    public async Task GetUserAsync_should_delegate()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "a@b.com") }));
        var expected = new User { Email = "a@b.com" };
        _dataStoreMock.Setup(d => d.GetUserAsync(principal)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetUserAsync(principal);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetUserAsync(principal), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_should_delegate()
    {
        var user = new User { Email = "a@b.com" };
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.ChangePasswordAsync(user, "old", "new")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.ChangePasswordAsync(user, "old", "new");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.ChangePasswordAsync(user, "old", "new"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_should_delegate()
    {
        var user = new User { Email = "a@b.com" };
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.CreateAsync(user, "pwd")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.CreateAsync(user, "pwd");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.CreateAsync(user, "pwd"), Times.Once);
    }

    [Fact]
    public async Task FindByEmailAsync_should_delegate()
    {
        var expected = new User { Email = "a@b.com" };
        _dataStoreMock.Setup(d => d.FindByEmailAsync("a@b.com")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.FindByEmailAsync("a@b.com");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.FindByEmailAsync("a@b.com"), Times.Once);
    }

    [Fact]
    public async Task GetUserIdAsync_should_delegate()
    {
        var principal = new ClaimsPrincipal();
        var expected = new User { Email = "user@example.com" };
        _dataStoreMock.Setup(d => d.GetUserIdAsync(principal)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetUserIdAsync(principal);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetUserIdAsync(principal), Times.Once);
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_should_delegate()
    {
        var user = new User { Email = "a@b.com" };
        _dataStoreMock.Setup(d => d.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("token");
        var sut = CreateSut();

        var result = await sut.GenerateEmailConfirmationTokenAsync(user);

        result.Should().Be("token");
        _dataStoreMock.Verify(d => d.GenerateEmailConfirmationTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_should_return_true_when_identity_succeeds()
    {
        var user = new User { Email = "a@b.com" };
        _dataStoreMock.Setup(d => d.ConfirmEmailAsync(user, "tkn")).ReturnsAsync(IdentityResult.Success);
        var sut = CreateSut();

        var result = await sut.ConfirmEmailAsync(user, "tkn");

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.ConfirmEmailAsync(user, "tkn"), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_should_return_false_when_identity_fails()
    {
        var user = new User { Email = "a@b.com" };
        var fail = IdentityResult.Failed(new IdentityError { Code = "x", Description = "y" });
        _dataStoreMock.Setup(d => d.ConfirmEmailAsync(user, "tkn")).ReturnsAsync(fail);
        var sut = CreateSut();

        var result = await sut.ConfirmEmailAsync(user, "tkn");

        result.Should().BeFalse();
        _dataStoreMock.Verify(d => d.ConfirmEmailAsync(user, "tkn"), Times.Once);
    }

    // ---------- CRUD-style and application methods ----------
    [Fact]
    public async Task GetAsync_should_delegate()
    {
        var id = Guid.NewGuid();
        var expected = new User { Id = id };
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
        var entity = new User { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.SaveAsync(entity)).ReturnsAsync(entity);
        var sut = CreateSut();

        var result = await sut.SaveAsync(entity);

        result.Should().BeSameAs(entity);
        _dataStoreMock.Verify(d => d.SaveAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_should_delegate()
    {
        var expected = new List<User> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetAllAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_by_entity_should_delegate()
    {
        var entity = new User { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.DeleteAsync(entity)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(entity);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetNewSpeakersAsync_should_delegate()
    {
        var expected = new List<User> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetNewSpeakersAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetNewSpeakersAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetNewSpeakersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetExperiencedSpeakersAsync_should_delegate()
    {
        var expected = new List<User> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetExperiencedSpeakersAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetExperiencedSpeakersAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetExperiencedSpeakersAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchSpeakersAsync_should_delegate_all_parameters()
    {
        var expected = new SpeakerSearchResult { Speakers = new List<User>(), RowCount = 0, PageSize = 10, TotalPages = 1, CurrentPage = 1 };
        _dataStoreMock.Setup(d => d.SearchSpeakersAsync("term", 1, 2, SpeakerSearchOrderBy.Name, 3, 10)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.SearchSpeakersAsync("term", 1, 2, SpeakerSearchOrderBy.Name, 3, 10);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.SearchSpeakersAsync("term", 1, 2, SpeakerSearchOrderBy.Name, 3, 10), Times.Once);
    }

    [Fact]
    public async Task GetSpeakersByExpertiseAsync_should_delegate()
    {
        var expected = new List<User> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetSpeakersByExpertiseAsync(5)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetSpeakersByExpertiseAsync(5);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetSpeakersByExpertiseAsync(5), Times.Once);
    }

    [Fact]
    public async Task AddSocialMediaLinkAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.AddSocialMediaLinkAsync(userId, "Twitter", "https://x.com/u")).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.AddSocialMediaLinkAsync(userId, "Twitter", "https://x.com/u");

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.AddSocialMediaLinkAsync(userId, "Twitter", "https://x.com/u"), Times.Once);
    }

    [Fact]
    public async Task RemoveSocialMediaLinkAsync_should_delegate()
    {
        _dataStoreMock.Setup(d => d.RemoveSocialMediaLinkAsync(123)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.RemoveSocialMediaLinkAsync(123);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.RemoveSocialMediaLinkAsync(123), Times.Once);
    }

    [Fact]
    public async Task AddExpertiseToUserAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.AddExpertiseToUserAsync(userId, 8)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.AddExpertiseToUserAsync(userId, 8);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.AddExpertiseToUserAsync(userId, 8), Times.Once);
    }

    [Fact]
    public async Task RemoveExpertiseFromUserAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.RemoveExpertiseFromUserAsync(userId, 8)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.RemoveExpertiseFromUserAsync(userId, 8);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.RemoveExpertiseFromUserAsync(userId, 8), Times.Once);
    }

    [Fact]
    public async Task EmptyAndAddExpertiseForUserAsync_should_throw_when_userId_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.EmptyAndAddExpertiseForUserAsync(Guid.Empty, new[] { 1, 2 });

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid user id");
        _dataStoreMock.Verify(d => d.EmptyAndAddExpertiseForUserAsync(It.IsAny<Guid>(), It.IsAny<int[]>()), Times.Never);
    }

    [Fact]
    public async Task EmptyAndAddExpertiseForUserAsync_should_delegate_when_userId_valid()
    {
        var id = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.EmptyAndAddExpertiseForUserAsync(id, new[] { 1, 2 })).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.EmptyAndAddExpertiseForUserAsync(id, new[] { 1, 2 });

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.EmptyAndAddExpertiseForUserAsync(id, new[] { 1, 2 }), Times.Once);
    }

    [Fact]
    public async Task EmptyAndAddSocialMediaForUserAsync_should_throw_when_userId_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.EmptyAndAddSocialMediaForUserAsync(Guid.Empty, new List<SocialMedia>());

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid user id");
        _dataStoreMock.Verify(d => d.EmptyAndAddSocialMediaForUserAsync(It.IsAny<Guid>(), It.IsAny<List<SocialMedia>>()), Times.Never);
    }

    [Fact]
    public async Task EmptyAndAddSocialMediaForUserAsync_should_delegate_when_userId_valid()
    {
        var id = Guid.NewGuid();
        var links = new List<SocialMedia> { new() { Id = 1, Platform = "X", Url = "https://x.com/user" } };
        _dataStoreMock.Setup(d => d.EmptyAndAddSocialMediaForUserAsync(id, links)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.EmptyAndAddSocialMediaForUserAsync(id, links);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.EmptyAndAddSocialMediaForUserAsync(id, links), Times.Once);
    }

    [Fact]
    public async Task GetUserExpertisesForUserAsync_should_delegate()
    {
        var id = Guid.NewGuid();
        var expected = new List<UserExpertise> { new() { UserId = id, ExpertiseId = 3 } };
        _dataStoreMock.Setup(d => d.GetUserExpertisesForUserAsync(id)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetUserExpertisesForUserAsync(id);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetUserExpertisesForUserAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetUserSocialMediaForUserAsync_should_delegate()
    {
        var id = Guid.NewGuid();
        var expected = new List<SocialMedia> { new() { Id = 1, Platform = "X", Url = "u" } };
        _dataStoreMock.Setup(d => d.GetUserSocialMediaForUserAsync(id)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetUserSocialMediaForUserAsync(id);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetUserSocialMediaForUserAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetStatisticsForApplicationAsync_should_delegate()
    {
        var expected = (newSpeakers: 2, experiencedSpeakers: 5, activeMentorships: 1);
        _dataStoreMock.Setup(d => d.GetStatisticsForApplicationAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetStatisticsForApplicationAsync();

        result.Should().Be(expected);
        _dataStoreMock.Verify(d => d.GetStatisticsForApplicationAsync(), Times.Once);
    }

    [Fact]
    public async Task GetFeaturedSpeakersAsync_should_delegate()
    {
        var expected = new List<User> { new() { Id = Guid.NewGuid() } };
        _dataStoreMock.Setup(d => d.GetFeaturedSpeakersAsync(3)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetFeaturedSpeakersAsync(3);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetFeaturedSpeakersAsync(3), Times.Once);
    }

    [Fact]
    public async Task GetSpeakerTypesAsync_should_delegate()
    {
        var expected = new List<SpeakerType> { new() { Id = 1, Name = "New" } };
        _dataStoreMock.Setup(d => d.GetSpeakerTypesAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetSpeakerTypesAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetSpeakerTypesAsync(), Times.Once);
    }
}