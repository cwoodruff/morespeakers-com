using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Managers.Tests;

public class UserManagerTests
{
    private readonly Mock<IUserDataStore> _dataStoreMock = new();
    private readonly Mock<IOpenGraphSpeakerProfileImageGenerator> _openGraphGeneratorMock = new();
    private readonly Mock<ILogger<UserManager>> _loggerMock = new();

    private UserManager CreateSut() => new(_dataStoreMock.Object, _openGraphGeneratorMock.Object, _loggerMock.Object);

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
    public async Task CreateAsync_should_delegate_and_queue_opengraph_when_headshot_exists()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", HeadshotUrl = "http://headshot" };
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.CreateAsync(user, "pwd")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.CreateAsync(user, "pwd");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.CreateAsync(user, "pwd"), Times.Once);
        _openGraphGeneratorMock.Verify(o => o.QueueSpeakerOpenGraphProfileImageCreation(user.Id, user.HeadshotUrl, user.FullName), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_should_not_queue_opengraph_when_creation_fails()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", HeadshotUrl = "http://headshot" };
        var expected = IdentityResult.Failed();
        _dataStoreMock.Setup(d => d.CreateAsync(user, "pwd")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.CreateAsync(user, "pwd");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.CreateAsync(user, "pwd"), Times.Once);
        _openGraphGeneratorMock.Verify(o => o.QueueSpeakerOpenGraphProfileImageCreation(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_should_not_queue_opengraph_when_headshot_missing()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", HeadshotUrl = "" };
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.CreateAsync(user, "pwd")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.CreateAsync(user, "pwd");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.CreateAsync(user, "pwd"), Times.Once);
        _openGraphGeneratorMock.Verify(o => o.QueueSpeakerOpenGraphProfileImageCreation(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
    public async Task SaveAsync_should_delegate_and_queue_opengraph_when_headshot_exists()
    {
        var entity = new User { Id = Guid.NewGuid(), HeadshotUrl = "http://headshot", FirstName = "Test", LastName = "User"};
        _dataStoreMock.Setup(d => d.SaveAsync(entity)).ReturnsAsync(entity);
        var sut = CreateSut();

        var result = await sut.SaveAsync(entity);

        result.Should().BeSameAs(entity);
        _dataStoreMock.Verify(d => d.SaveAsync(entity), Times.Once);
        _openGraphGeneratorMock.Verify(o => o.QueueSpeakerOpenGraphProfileImageCreation(entity.Id, entity.HeadshotUrl, entity.FullName), Times.Once);
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
        var expertiseIds = new List<int> { 2 };
        _dataStoreMock.Setup(d => d.SearchSpeakersAsync("term", 1, expertiseIds, SpeakerSearchOrderBy.Name, 3, 10)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.SearchSpeakersAsync("term", 1, expertiseIds, SpeakerSearchOrderBy.Name, 3, 10);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.SearchSpeakersAsync("term", 1, expertiseIds, SpeakerSearchOrderBy.Name, 3, 10), Times.Once);
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
    public async Task AddUserSocialMediaSiteAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        var link = new UserSocialMediaSite
        {
            UserId = userId,
            SocialMediaSiteId = 2,
            SocialId = "myhandle",
            User = new User { Id = userId },
            SocialMediaSite = new SocialMediaSite { Id = 2, Name = "X", Icon = "x", UrlFormat = "https://x.com/{0}" }
        };
        _dataStoreMock.Setup(d => d.AddUserSocialMediaSiteAsync(userId, link)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.AddUserSocialMediaSiteAsync(userId, link);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.AddUserSocialMediaSiteAsync(userId, link), Times.Once);
    }

    [Fact]
    public async Task RemoveUserSocialMediaSiteAsync_should_delegate()
    {
        _dataStoreMock.Setup(d => d.RemoveUserSocialMediaSiteAsync(123)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.RemoveUserSocialMediaSiteAsync(123);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.RemoveUserSocialMediaSiteAsync(123), Times.Once);
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
    public async Task GetUserSocialMediaSitesAsync_should_delegate()
    {
        var id = Guid.NewGuid();
        var expected = new List<UserSocialMediaSite>
        {
            new()
            {
                Id = 10,
                UserId = id,
                SocialMediaSiteId = 1,
                SocialId = "user",
                User = new User { Id = id },
                SocialMediaSite = new SocialMediaSite { Id = 1, Name = "X", Icon = "x", UrlFormat = "https://x.com/{0}" }
            }
        };
        _dataStoreMock.Setup(d => d.GetUserSocialMediaSitesAsync(id)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetUserSocialMediaSitesAsync(id);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetUserSocialMediaSitesAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetUserSocialMediaSitesAsync_should_throw_when_userId_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.GetUserSocialMediaSitesAsync(Guid.Empty);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid user id");
        _dataStoreMock.Verify(d => d.GetUserSocialMediaSitesAsync(It.IsAny<Guid>()), Times.Never);
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
    public async Task GeneratePasswordResetTokenAsync_should_delegate()
    {
        var sut = CreateSut();
        var user = new User { Id = Guid.NewGuid() };
        _dataStoreMock.Setup(d => d.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");

        var result = await sut.GeneratePasswordResetTokenAsync(user);

        result.Should().Be("token");
        _dataStoreMock.Verify(d => d.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_should_delegate()
    {
        var sut = CreateSut();
        var user = new User { Id = Guid.NewGuid() };
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.ResetPasswordAsync(user, "token", "new")).ReturnsAsync(expected);

        var result = await sut.ResetPasswordAsync(user, "token", "new");

        result.Should().Be(expected);
        _dataStoreMock.Verify(d => d.ResetPasswordAsync(user, "token", "new"), Times.Once);
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

    [Fact]
    public async Task AddToRolesAsync_should_delegate()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var roles = new[] { "Role1", "Role2" };
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.AddToRolesAsync(userId, roles)).ReturnsAsync(expected);

        var result = await sut.AddToRolesAsync(userId, roles);

        result.Should().Be(expected);
        _dataStoreMock.Verify(d => d.AddToRolesAsync(userId, roles), Times.Once);
    }

    [Fact]
    public async Task RemoveFromRolesAsync_should_delegate()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var roles = new[] { "Role1", "Role2" };
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.RemoveFromRolesAsync(userId, roles)).ReturnsAsync(expected);

        var result = await sut.RemoveFromRolesAsync(userId, roles);

        result.Should().Be(expected);
        _dataStoreMock.Verify(d => d.RemoveFromRolesAsync(userId, roles), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdatePasskeyAsync_should_delegate()
    {
        var sut = CreateSut();
        var user = new User { Id = Guid.NewGuid() };
        var passkey = new UserPasskeyInfo(new byte[10], new byte[10], DateTimeOffset.UtcNow, 0, ["transport"],true, true, true, new byte[10], new byte[10] );
        var expected = IdentityResult.Success;
        _dataStoreMock.Setup(d => d.AddOrUpdatePasskeyAsync(user, passkey)).ReturnsAsync(expected);

        var result = await sut.AddOrUpdatePasskeyAsync(user, passkey);

        result.Should().Be(expected);
        _dataStoreMock.Verify(d => d.AddOrUpdatePasskeyAsync(user, passkey), Times.Once);
    }

    [Fact]
    public async Task GetUserPasskeysAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        var expected = new List<UserPasskey> { new() { Id = "pk1", UserId = userId, FriendlyName = "Browser" } };
        _dataStoreMock.Setup(d => d.GetUserPasskeysAsync(userId)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetUserPasskeysAsync(userId);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetUserPasskeysAsync(userId), Times.Once);
    }

    [Fact]
    public async Task RemovePasskeyAsync_should_decode_and_delegate()
    {
        var userId = Guid.NewGuid();
        var credentialBytes = new byte[] { 1, 2, 3 };
        var credentialId = WebEncoders.Base64UrlEncode(credentialBytes);
        _dataStoreMock.Setup(d => d.RemovePasskeyAsync(userId, It.IsAny<byte[]>())).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.RemovePasskeyAsync(userId, credentialId);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.RemovePasskeyAsync(userId, It.Is<byte[]>(b => b.SequenceEqual(credentialBytes))), Times.Once);
    }

    [Fact]
    public async Task RemovePasskeyAsync_should_return_false_on_invalid_base64()
    {
        var userId = Guid.NewGuid();
        var sut = CreateSut();

        var result = await sut.RemovePasskeyAsync(userId, "not-base64");

        result.Should().BeFalse();
        _dataStoreMock.Verify(d => d.RemovePasskeyAsync(It.IsAny<Guid>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_should_not_queue_opengraph_when_headshot_missing()
    {
        var entity = new User { Id = Guid.NewGuid(), HeadshotUrl = "" };
        _dataStoreMock.Setup(d => d.SaveAsync(entity)).ReturnsAsync(entity);
        var sut = CreateSut();

        var result = await sut.SaveAsync(entity);

        result.Should().BeSameAs(entity);
        _openGraphGeneratorMock.Verify(o => o.QueueSpeakerOpenGraphProfileImageCreation(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AdminSearchUsersAsync_should_delegate()
    {
        var filter = new UserAdminFilter { Query = "test" };
        var sort = new UserAdminSort { By = UserAdminSortBy.Email, Direction = SortDirection.Desc };
        var expected = new PagedResult<UserListRow> { Items = new List<UserListRow>(), TotalCount = 0, Page = 1, PageSize = 10 };
        _dataStoreMock.Setup(d => d.AdminSearchUsersAsync(filter, sort, 1, 10)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.AdminSearchUsersAsync(filter, sort, 1, 10);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.AdminSearchUsersAsync(filter, sort, 1, 10), Times.Once);
    }

    [Fact]
    public async Task GetAllRoleNamesAsync_should_delegate()
    {
        var expected = new List<string> { "Admin" };
        _dataStoreMock.Setup(d => d.GetAllRoleNamesAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllRoleNamesAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllRoleNamesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRolesForUserAsync_should_delegate()
    {
        var userId = Guid.NewGuid();
        var expected = new List<string> { "User" };
        _dataStoreMock.Setup(d => d.GetRolesForUserAsync(userId)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetRolesForUserAsync(userId);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetRolesForUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task EnableLockoutAsync_should_return_false_for_empty_user()
    {
        var sut = CreateSut();

        var result = await sut.EnableLockoutAsync(Guid.Empty, true);

        result.Should().BeFalse();
        _dataStoreMock.Verify(d => d.EnableLockoutAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SetLockoutEndAsync_should_return_false_for_empty_user()
    {
        var sut = CreateSut();

        var result = await sut.SetLockoutEndAsync(Guid.Empty, DateTimeOffset.UtcNow);

        result.Should().BeFalse();
        _dataStoreMock.Verify(d => d.SetLockoutEndAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset?>()), Times.Never);
    }

    [Fact]
    public async Task UnlockAsync_should_return_false_for_empty_user()
    {
        var sut = CreateSut();

        var result = await sut.UnlockAsync(Guid.Empty);

        result.Should().BeFalse();
        _dataStoreMock.Verify(d => d.UnlockAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task EnableLockoutAsync_should_delegate_for_valid_user()
    {
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.EnableLockoutAsync(userId, true)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.EnableLockoutAsync(userId, true);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.EnableLockoutAsync(userId, true), Times.Once);
    }

    [Fact]
    public async Task SetLockoutEndAsync_should_delegate_for_valid_user()
    {
        var userId = Guid.NewGuid();
        var lockout = DateTimeOffset.UtcNow;
        _dataStoreMock.Setup(d => d.SetLockoutEndAsync(userId, lockout)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.SetLockoutEndAsync(userId, lockout);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.SetLockoutEndAsync(userId, lockout), Times.Once);
    }

    [Fact]
    public async Task UnlockAsync_should_delegate_for_valid_user()
    {
        var userId = Guid.NewGuid();
        _dataStoreMock.Setup(d => d.UnlockAsync(userId)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.UnlockAsync(userId);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.UnlockAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserCountInRoleAsync_should_trim_role_name()
    {
        _dataStoreMock.Setup(d => d.GetUserCountInRoleAsync("Admin")).ReturnsAsync(3);
        var sut = CreateSut();

        var result = await sut.GetUserCountInRoleAsync(" Admin ");

        result.Should().Be(3);
        _dataStoreMock.Verify(d => d.GetUserCountInRoleAsync("Admin"), Times.Once);
    }

    [Fact]
    public async Task GetUserCountInRoleAsync_should_return_zero_for_empty()
    {
        var sut = CreateSut();

        var result = await sut.GetUserCountInRoleAsync(" ");

        result.Should().Be(0);
        _dataStoreMock.Verify(d => d.GetUserCountInRoleAsync(It.IsAny<string>()), Times.Never);
    }
}