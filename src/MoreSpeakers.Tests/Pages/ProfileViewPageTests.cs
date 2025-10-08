using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Pages.Profile;

namespace MoreSpeakers.Tests.Pages;

public class ProfileViewPageTests : TestBase
{
    private readonly ViewModel _pageModel;

    public ProfileViewPageTests()
    {
        _pageModel = new ViewModel(Context);
    }

    [Fact]
    public async Task OnGetAsync_WithValidUserId_ShouldReturnPageResult()
    {
        // Arrange
        var user = GetNewSpeaker();
        _pageModel.Id = user.Id;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.ProfileUser.Should().NotBeNull();
        _pageModel.ProfileUser!.Id.Should().Be(user.Id);
        _pageModel.ProfileUser.SpeakerType.Should().NotBeNull();
    }

    [Fact]
    public async Task OnGetAsync_WithValidUserId_ShouldLoadUserExpertise()
    {
        // Arrange
        var user = GetNewSpeaker();
        _pageModel.Id = user.Id;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.UserExpertise.Should().NotBeEmpty();
        _pageModel.UserExpertise.Should().OnlyContain(ue => ue.UserId == user.Id);
        _pageModel.UserExpertise.Should().OnlyContain(ue => ue.Expertise != null);
    }

    [Fact]
    public async Task OnGetAsync_WithValidUserId_ShouldLoadSocialMedia()
    {
        // Arrange
        var user = GetNewSpeaker();
        _pageModel.Id = user.Id;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.SocialMedia.Should().NotBeEmpty();
        _pageModel.SocialMedia.Should().OnlyContain(sm => sm.UserId == user.Id);
    }

    [Fact]
    public async Task OnGetAsync_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Arrange
        _pageModel.Id = Guid.Empty;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _pageModel.ProfileUser.Should().BeNull();
    }

    [Fact]
    public async Task OnGetAsync_WithNonExistentUserId_ShouldReturnPageWithNullUser()
    {
        // Arrange
        _pageModel.Id = Guid.NewGuid(); // Random non-existent ID

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.ProfileUser.Should().BeNull();
        _pageModel.UserExpertise.Should().BeEmpty();
        _pageModel.SocialMedia.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_ShouldIncludeSpeakerTypeNavigation()
    {
        // Arrange
        var user = GetExperiencedSpeaker();
        _pageModel.Id = user.Id;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.ProfileUser.Should().NotBeNull();
        _pageModel.ProfileUser!.SpeakerType.Should().NotBeNull();
        _pageModel.ProfileUser.SpeakerType.Name.Should().Be("ExperiencedSpeaker");
    }

    [Fact]
    public async Task OnGetAsync_ShouldIncludeExpertiseNavigation()
    {
        // Arrange
        var user = GetNewSpeaker();
        _pageModel.Id = user.Id;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.UserExpertise.Should().NotBeEmpty();
        _pageModel.UserExpertise.Should().OnlyContain(ue => ue.Expertise != null);
        
        var firstExpertise = _pageModel.UserExpertise.First();
        firstExpertise.Expertise.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ViewModel_ShouldInheritFromPageModel()
    {
        // Assert
        _pageModel.Should().BeAssignableTo<PageModel>();
    }

    [Fact]
    public void ViewModel_Properties_ShouldHaveDefaultValues()
    {
        // Act
        var newPageModel = new ViewModel(Context);

        // Assert
        newPageModel.Id.Should().Be(Guid.Empty);
        newPageModel.ProfileUser.Should().BeNull();
        newPageModel.UserExpertise.Should().NotBeNull().And.BeEmpty();
        newPageModel.SocialMedia.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_WithUserHavingMultipleExpertiseAreas_ShouldLoadAll()
    {
        // Arrange
        var user = GetExperiencedSpeaker(); // This user has multiple expertise areas from test setup
        _pageModel.Id = user.Id;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.UserExpertise.Should().HaveCountGreaterThan(1);
        _pageModel.UserExpertise.Should().OnlyContain(ue => ue.UserId == user.Id);
    }

    [Fact]
    public async Task OnGetAsync_WithUserHavingMultipleSocialMediaLinks_ShouldLoadAll()
    {
        // Arrange
        var user = GetExperiencedSpeaker();
        
        // Add additional social media for testing
        var additionalSocialMedia = new SocialMedia
        {
            UserId = user.Id,
            Platform = "GitHub",
            Url = "https://github.com/janesmith",
            CreatedDate = DateTime.UtcNow
        };
        Context.SocialMedia.Add(additionalSocialMedia);
        await Context.SaveChangesAsync();
        
        _pageModel.Id = user.Id;

        // Act
        var result = await _pageModel.OnGetAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _pageModel.SocialMedia.Count().Should().BeGreaterThanOrEqualTo(2);
        _pageModel.SocialMedia.Should().OnlyContain(sm => sm.UserId == user.Id);
    }

    [Fact]
    public void IdProperty_ShouldSupportGetBinding()
    {
        // Arrange
        var property = typeof(ViewModel).GetProperty("Id");

        // Assert
        property.Should().NotBeNull();
        
        var bindPropertyAttribute = property!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.BindPropertyAttribute), false)
            .Cast<Microsoft.AspNetCore.Mvc.BindPropertyAttribute>()
            .FirstOrDefault();
            
        bindPropertyAttribute.Should().NotBeNull();
        bindPropertyAttribute!.SupportsGet.Should().BeTrue();
    }
}