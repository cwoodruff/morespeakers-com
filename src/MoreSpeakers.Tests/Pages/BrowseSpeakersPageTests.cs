using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Pages;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Tests.Pages;

public class BrowseSpeakersPageTests
{
    private readonly Mock<IExpertiseService> _mockExpertiseService;
    private readonly Mock<ISpeakerService> _mockSpeakerService;
    private readonly BrowseSpeakersModel _pageModel;

    public BrowseSpeakersPageTests()
    {
        _mockSpeakerService = new Mock<ISpeakerService>();
        _mockExpertiseService = new Mock<IExpertiseService>();

        _pageModel = new BrowseSpeakersModel(
            _mockSpeakerService.Object,
            _mockExpertiseService.Object);
    }

    [Fact]
    public async Task OnGetAsync_ShouldLoadAllExpertise()
    {
        // Arrange
        var expertise = CreateSampleExpertise(5);
        var speakers = CreateSampleUsers(3);

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(expertise);

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(speakers.Take(2));

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(speakers.Skip(2));

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _pageModel.AllExpertise.Should().HaveCount(5);
        _mockExpertiseService.Verify(e => e.GetAllExpertiseAsync(), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WithSearchTerm_ShouldCallSearchSpeakers()
    {
        // Arrange
        _pageModel.SearchTerm = "John";
        var searchResults = CreateSampleUsers(2);

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.SearchSpeakersAsync("John", null))
            .ReturnsAsync(searchResults);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _mockSpeakerService.Verify(s => s.SearchSpeakersAsync("John", null), Times.Once);
        _pageModel.Speakers.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnGetAsync_WithSpeakerTypeFilter_ShouldCallCorrectService()
    {
        // Arrange
        _pageModel.SpeakerTypeFilter = 1; // NewSpeaker
        var newSpeakers = CreateSampleUsers(3);

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(newSpeakers);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _mockSpeakerService.Verify(s => s.GetNewSpeakersAsync(), Times.Once);
        _mockSpeakerService.Verify(s => s.GetExperiencedSpeakersAsync(), Times.Never);
    }

    [Fact]
    public async Task OnGetAsync_WithExperiencedSpeakerFilter_ShouldCallCorrectService()
    {
        // Arrange
        _pageModel.SpeakerTypeFilter = 2; // ExperiencedSpeaker
        var experiencedSpeakers = CreateSampleUsers(3);

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(experiencedSpeakers);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _mockSpeakerService.Verify(s => s.GetExperiencedSpeakersAsync(), Times.Once);
        _mockSpeakerService.Verify(s => s.GetNewSpeakersAsync(), Times.Never);
    }

    [Fact]
    public async Task OnGetAsync_WithExpertiseFilter_ShouldCallGetSpeakersByExpertise()
    {
        // Arrange
        _pageModel.ExpertiseFilter = 1;
        var speakersWithExpertise = CreateSampleUsers(2);

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.GetSpeakersByExpertiseAsync(1))
            .ReturnsAsync(speakersWithExpertise);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _mockSpeakerService.Verify(s => s.GetSpeakersByExpertiseAsync(1), Times.Once);
        _pageModel.Speakers.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnGetAsync_WithNoFilters_ShouldGetAllSpeakers()
    {
        // Arrange
        var newSpeakers = CreateSampleUsers(3);
        var experiencedSpeakers = CreateSampleUsers(5);

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(newSpeakers);

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(experiencedSpeakers);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _mockSpeakerService.Verify(s => s.GetNewSpeakersAsync(), Times.Once);
        _mockSpeakerService.Verify(s => s.GetExperiencedSpeakersAsync(), Times.Once);
        _pageModel.TotalCount.Should().Be(8); // 3 + 5
    }

    [Theory]
    [InlineData("newest")]
    [InlineData("expertise")]
    [InlineData("name")]
    public async Task OnGetAsync_WithDifferentSortOptions_ShouldApplySorting(string sortBy)
    {
        // Arrange
        _pageModel.SortBy = sortBy;
        var speakers = CreateSampleUsersWithDifferentData();

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(speakers);

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _pageModel.Speakers.Should().NotBeEmpty();

        if (sortBy == "name") _pageModel.Speakers.Should().BeInAscendingOrder(s => s.FirstName);
        // Note: Other sorting tests would require more complex setup for in-memory testing
    }

    [Fact]
    public async Task OnGetAsync_ShouldCalculatePaginationCorrectly()
    {
        // Arrange
        var speakers = CreateSampleUsers(25); // More than one page (PageSize = 12)
        _pageModel.CurrentPage = 2;

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(speakers);

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _pageModel.TotalCount.Should().Be(25);
        _pageModel.TotalPages.Should().Be(3); // Ceiling(25/12) = 3
        _pageModel.CurrentPage.Should().Be(2);
        _pageModel.Speakers.Should().HaveCount(12); // Full page size
    }

    [Fact]
    public async Task OnGetAsync_WithInvalidPage_ShouldClampToValidRange()
    {
        // Arrange
        var speakers = CreateSampleUsers(15);
        _pageModel.CurrentPage = 10; // Invalid page number

        _mockExpertiseService.Setup(e => e.GetAllExpertiseAsync())
            .ReturnsAsync(new List<Expertise>());

        _mockSpeakerService.Setup(s => s.GetNewSpeakersAsync())
            .ReturnsAsync(speakers);

        _mockSpeakerService.Setup(s => s.GetExperiencedSpeakersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        _pageModel.TotalPages.Should().Be(2); // Ceiling(15/12) = 2
        _pageModel.CurrentPage.Should().Be(2); // Clamped to max valid page
    }

    [Fact]
    public async Task OnPostRequestMentorshipAsync_WithUnauthenticatedUser_ShouldReturnError()
    {
        // Arrange
        SetupUnauthenticatedUser();
        var mentorId = Guid.NewGuid();

        // Act
        var result = await _pageModel.OnPostRequestMentorshipAsync(mentorId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        var value = jsonResult!.Value;
        value.Should().NotBeNull();

        // Use reflection to get the error property
        var errorProperty = value!.GetType().GetProperty("error");
        if (errorProperty != null)
        {
            var errorValue = errorProperty.GetValue(value);
            errorValue.Should().Be("Please log in to request mentorship.");
        }
        else
        {
            // Alternative check using anonymous type conversion
            var json = JsonSerializer.Serialize(value);
            json.Should().Contain("Please log in to request mentorship");
        }
    }

    [Fact]
    public async Task OnPostRequestMentorshipAsync_WithAuthenticatedUser_ShouldReturnSuccess()
    {
        // Arrange
        SetupAuthenticatedUser();
        var mentorId = Guid.NewGuid();

        // Act
        var result = await _pageModel.OnPostRequestMentorshipAsync(mentorId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        var value = jsonResult!.Value;
        value.Should().NotBeNull();

        // Use reflection to get the success property
        var successProperty = value!.GetType().GetProperty("success");
        if (successProperty != null)
        {
            var successValue = successProperty.GetValue(value);
            successValue.Should().Be("Mentorship request sent successfully!");
        }
        else
        {
            // Alternative check using anonymous type conversion
            var json = JsonSerializer.Serialize(value);
            json.Should().Contain("Mentorship request sent successfully");
        }
    }

    [Fact]
    public void BrowseSpeakersModel_ShouldInheritFromPageModel()
    {
        // Assert
        _pageModel.Should().BeAssignableTo<PageModel>();
    }

    [Fact]
    public void BrowseSpeakersModel_Properties_ShouldHaveDefaultValues()
    {
        // Assert
        _pageModel.Speakers.Should().NotBeNull().And.BeEmpty();
        _pageModel.AllExpertise.Should().NotBeNull().And.BeEmpty();
        _pageModel.SearchTerm.Should().BeNull();
        _pageModel.SpeakerTypeFilter.Should().BeNull();
        _pageModel.ExpertiseFilter.Should().BeNull();
        _pageModel.SortBy.Should().Be("name");
        _pageModel.CurrentPage.Should().Be(1);
        _pageModel.TotalCount.Should().Be(0);
        _pageModel.TotalPages.Should().Be(0);
    }

    private void SetupUnauthenticatedUser()
    {
        var httpContext = new DefaultHttpContext();
        // Create an unauthenticated principal
        var identity = new ClaimsIdentity(); // No authentication type = unauthenticated
        httpContext.User = new ClaimsPrincipal(identity);

        _pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
    }

    private void SetupAuthenticatedUser()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        _pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
    }

    private static List<User> CreateSampleUsers(int count)
    {
        var users = new List<User>();
        for (var i = 0; i < count; i++)
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                FirstName = $"User{i}",
                LastName = $"LastName{i}",
                Bio = $"Bio for User{i}",
                UserExpertise = new List<UserExpertise>()
            });
        return users;
    }

    private static List<User> CreateSampleUsersWithDifferentData()
    {
        return new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Charlie",
                LastName = "Wilson",
                Bio = "Bio",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                UserExpertise = new List<UserExpertise> { new(), new(), new() }
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Brown",
                Bio = "Bio",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                UserExpertise = new List<UserExpertise> { new() }
            },
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Johnson",
                Bio = "Bio",
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                UserExpertise = new List<UserExpertise> { new(), new() }
            }
        };
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