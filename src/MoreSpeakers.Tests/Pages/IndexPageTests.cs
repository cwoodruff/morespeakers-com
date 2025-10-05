using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Pages;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Tests.Pages;

public class IndexPageTests
{
    private readonly Mock<IExpertiseService> _mockExpertiseService;
    private readonly Mock<IMentorshipService> _mockMentorshipService;
    private readonly Mock<ISpeakerService> _mockSpeakerService;
    private readonly IndexModel _pageModel;

    public IndexPageTests()
    {
        _mockSpeakerService = new Mock<ISpeakerService>();
        _mockMentorshipService = new Mock<IMentorshipService>();
        _mockExpertiseService = new Mock<IExpertiseService>();

        _pageModel = new IndexModel(
            _mockSpeakerService.Object,
            _mockMentorshipService.Object,
            _mockExpertiseService.Object);
    }

    [Fact]
    public async Task OnGetAsync_ShouldLoadStatisticsCorrectly()
    {
        // Arrange
        var newSpeakers = CreateSampleUsers(3, SpeakerTypeEnum.NewSpeaker);
        var experiencedSpeakers = CreateSampleUsers(5, SpeakerTypeEnum.ExperiencedSpeaker);
        var activeMentorships = CreateSampleMentorships(7);
        var popularExpertise = CreateSampleExpertise(8);

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(newSpeakers);

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(experiencedSpeakers);

        _mockMentorshipService.Setup(m => m.GetActiveMentorshipsAsync())
            .ReturnsAsync(activeMentorships);

        _mockExpertiseService.Setup(e => e.GetPopularExpertiseAsync(8))
            .ReturnsAsync(popularExpertise);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _pageModel.NewSpeakersCount.Should().Be(3);
        _pageModel.ExperiencedSpeakersCount.Should().Be(5);
        _pageModel.ActiveMentorshipsCount.Should().Be(7);
        _pageModel.PopularExpertise.Should().HaveCount(8);
    }

    [Fact]
    public async Task OnGetAsync_ShouldFilterFeaturedSpeakersCorrectly()
    {
        // Arrange
        var experiencedSpeakers = new List<User>
        {
            CreateUser("John", "Doe", SpeakerTypeEnum.ExperiencedSpeaker, "Experienced speaker", 3),
            CreateUser("Jane", "Smith", SpeakerTypeEnum.ExperiencedSpeaker, "", 2), // No bio - should be filtered out
            CreateUser("Bob", "Johnson", SpeakerTypeEnum.ExperiencedSpeaker, "Another speaker", 5),
            CreateUser("Alice", "Brown", SpeakerTypeEnum.ExperiencedSpeaker, "Great speaker",
                0), // No expertise - should be filtered out
            CreateUser("Charlie", "Wilson", SpeakerTypeEnum.ExperiencedSpeaker, "Top speaker", 4)
        };

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(new List<User>());

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(experiencedSpeakers);

        _mockMentorshipService.Setup(m => m.GetActiveMentorshipsAsync())
            .ReturnsAsync(new List<Mentorship>());

        _mockExpertiseService.Setup(e => e.GetPopularExpertiseAsync(8))
            .ReturnsAsync(new List<Expertise>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _pageModel.FeaturedSpeakers.Should().HaveCount(3); // Only speakers with bio and expertise
        _pageModel.FeaturedSpeakers.Should().NotContain(s => string.IsNullOrEmpty(s.Bio));
        _pageModel.FeaturedSpeakers.Should().NotContain(s => !s.UserExpertise.Any());

        // Should be ordered by expertise count (descending)
        var featuredList = _pageModel.FeaturedSpeakers.ToList();
        featuredList[0].LastName.Should().Be("Johnson"); // 5 expertise
        featuredList[1].LastName.Should().Be("Wilson"); // 4 expertise
        featuredList[2].LastName.Should().Be("Doe"); // 3 expertise
    }

    [Fact]
    public async Task OnGetAsync_ShouldLimitFeaturedSpeakersToSix()
    {
        // Arrange
        var experiencedSpeakers = CreateSampleUsers(10, SpeakerTypeEnum.ExperiencedSpeaker)
            .Select(u =>
            {
                u.Bio = "Has bio";
                u.UserExpertise = new List<UserExpertise> { new() { ExpertiseId = 1 } };
                return u;
            }).ToList();

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(new List<User>());

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(experiencedSpeakers);

        _mockMentorshipService.Setup(m => m.GetActiveMentorshipsAsync())
            .ReturnsAsync(new List<Mentorship>());

        _mockExpertiseService.Setup(e => e.GetPopularExpertiseAsync(8))
            .ReturnsAsync(new List<Expertise>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _pageModel.FeaturedSpeakers.Should().HaveCount(6);
    }

    [Fact]
    public async Task OnGetAsync_ShouldCallAllServiceMethods()
    {
        // Arrange
        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(new List<User>());

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(new List<User>());

        _mockMentorshipService.Setup(m => m.GetActiveMentorshipsAsync())
            .ReturnsAsync(new List<Mentorship>());

        _mockExpertiseService.Setup(e => e.GetPopularExpertiseAsync(8))
            .ReturnsAsync(new List<Expertise>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _mockSpeakerService.Verify(s => s.GetNewSpeakersAsync(), Times.Once);
        _mockSpeakerService.Verify(s => s.GetExperiencedSpeakersAsync(), Times.Once);
        _mockMentorshipService.Verify(m => m.GetActiveMentorshipsAsync(), Times.Once);
        _mockExpertiseService.Verify(e => e.GetPopularExpertiseAsync(8), Times.Once);
    }

    [Fact]
    public void IndexModel_ShouldInheritFromPageModel()
    {
        // Assert
        _pageModel.Should().BeAssignableTo<PageModel>();
    }

    [Fact]
    public void IndexModel_Properties_ShouldHaveDefaultValues()
    {
        // Assert
        _pageModel.NewSpeakersCount.Should().Be(0);
        _pageModel.ExperiencedSpeakersCount.Should().Be(0);
        _pageModel.ActiveMentorshipsCount.Should().Be(0);
        _pageModel.FeaturedSpeakers.Should().NotBeNull().And.BeEmpty();
        _pageModel.PopularExpertise.Should().NotBeNull().And.BeEmpty();
    }

    private static List<User> CreateSampleUsers(int count, SpeakerTypeEnum speakerType)
    {
        var users = new List<User>();
        for (var i = 0; i < count; i++) users.Add(CreateUser($"User{i}", $"LastName{i}", speakerType));
        return users;
    }

    private static User CreateUser(string firstName, string lastName, SpeakerTypeEnum speakerType,
        string bio = "Sample bio", int expertiseCount = 1)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Bio = bio,
            SpeakerTypeId = (int)speakerType,
            SpeakerType = new SpeakerType
            {
                Id = (int)speakerType,
                Name = speakerType.ToString()
            },
            UserExpertise = new List<UserExpertise>()
        };

        // Add expertise
        for (var i = 0; i < expertiseCount; i++)
            user.UserExpertise.Add(new UserExpertise
            {
                UserId = user.Id,
                ExpertiseId = i + 1,
                Expertise = new Expertise { Id = i + 1, Name = $"Expertise{i + 1}" }
            });

        return user;
    }

    private static List<Mentorship> CreateSampleMentorships(int count)
    {
        var mentorships = new List<Mentorship>();
        for (var i = 0; i < count; i++)
            mentorships.Add(new Mentorship
            {
                Id = Guid.NewGuid(),
                Status = MentorshipStatus.Active,
                MenteeId = Guid.NewGuid(),
                MentorId = Guid.NewGuid()
            });
        return mentorships;
    }

    private static List<Expertise> CreateSampleExpertise(int count)
    {
        var expertise = new List<Expertise>();
        for (var i = 0; i < count; i++)
            expertise.Add(new Expertise
            {
                Id = i + 1,
                Name = $"Technology{i + 1}",
                Description = $"Description for Technology{i + 1}"
            });
        return expertise;
    }
}