using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using MoreSpeakers.Web.Models;

namespace MoreSpeakers.Tests.Models;

public class UserTests
{
    [Fact]
    public void User_FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void User_IsNewSpeaker_ShouldReturnTrueForNewSpeaker()
    {
        // Arrange
        var user = new User
        {
            SpeakerType = new SpeakerType { Name = "NewSpeaker" }
        };

        // Act
        var isNewSpeaker = user.IsNewSpeaker;

        // Assert
        isNewSpeaker.Should().BeTrue();
    }

    [Fact]
    public void User_IsNewSpeaker_ShouldReturnFalseForExperiencedSpeaker()
    {
        // Arrange
        var user = new User
        {
            SpeakerType = new SpeakerType { Name = "ExperiencedSpeaker" }
        };

        // Act
        var isNewSpeaker = user.IsNewSpeaker;

        // Assert
        isNewSpeaker.Should().BeFalse();
    }

    [Fact]
    public void User_IsExperiencedSpeaker_ShouldReturnTrueForExperiencedSpeaker()
    {
        // Arrange
        var user = new User
        {
            SpeakerType = new SpeakerType { Name = "ExperiencedSpeaker" }
        };

        // Act
        var isExperiencedSpeaker = user.IsExperiencedSpeaker;

        // Assert
        isExperiencedSpeaker.Should().BeTrue();
    }

    [Fact]
    public void User_IsExperiencedSpeaker_ShouldReturnFalseForNewSpeaker()
    {
        // Arrange
        var user = new User
        {
            SpeakerType = new SpeakerType { Name = "NewSpeaker" }
        };

        // Act
        var isExperiencedSpeaker = user.IsExperiencedSpeaker;

        // Assert
        isExperiencedSpeaker.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void User_FirstName_ShouldFailValidationWhenEmpty(string firstName)
    {
        // Arrange
        var user = new User
        {
            FirstName = firstName,
            LastName = "Doe",
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = "123-456-7890"
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("FirstName"));
    }

    [Fact]
    public void User_FirstName_ShouldFailValidationWhenTooLong()
    {
        // Arrange
        var user = new User
        {
            FirstName = new string('A', 101), // Exceeds MaxLength of 100
            LastName = "Doe",
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = "123-456-7890"
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("FirstName"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void User_LastName_ShouldFailValidationWhenEmpty(string lastName)
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = lastName,
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = "123-456-7890"
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("LastName"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void User_Bio_ShouldFailValidationWhenEmpty(string bio)
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = bio,
            Goals = "Test goals",
            PhoneNumber = "123-456-7890"
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Bio"));
    }

    [Fact]
    public void User_Bio_ShouldFailValidationWhenTooLong()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = new string('A', 6001), // Exceeds MaxLength of 6000
            Goals = "Test goals",
            PhoneNumber = "123-456-7890"
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Bio"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void User_Goals_ShouldFailValidationWhenEmpty(string goals)
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = "Test bio",
            Goals = goals,
            PhoneNumber = "123-456-7890"
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Goals"));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("just-text")]
    public void User_SessionizeUrl_ShouldFailValidationWhenInvalidUrl(string invalidUrl)
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = "123-456-7890",
            SessionizeUrl = invalidUrl
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("SessionizeUrl"));
    }

    [Theory]
    [InlineData("https://sessionize.com/john-doe")]
    [InlineData("http://example.com")]
    [InlineData(null)]
    public void User_SessionizeUrl_ShouldPassValidationWhenValidOrEmpty(string sessionizeUrl)
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = "123-456-7890",
            SessionizeUrl = sessionizeUrl
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().NotContain(vr => vr.MemberNames.Contains("SessionizeUrl"));
    }

    [Fact]
    public void User_SessionizeUrl_ShouldFailValidationWhenEmpty()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = "123-456-7890",
            SessionizeUrl = "" // Empty string fails URL validation
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert - Empty string fails URL validation in .NET
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("SessionizeUrl"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void User_PhoneNumber_ShouldFailValidationWhenInvalid(string phoneNumber)
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = phoneNumber
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("PhoneNumber"));
    }

    [Theory]
    [InlineData("123-456-7890")]
    [InlineData("(555) 123-4567")]
    [InlineData("555.123.4567")]
    [InlineData("+1-555-123-4567")]
    [InlineData("5551234567")]
    public void User_PhoneNumber_ShouldPassValidationWhenValid(string phoneNumber)
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Bio = "Test bio",
            Goals = "Test goals",
            PhoneNumber = phoneNumber
        };

        // Act
        var validationResults = ValidateModel(user);

        // Assert
        validationResults.Should().NotContain(vr => vr.MemberNames.Contains("PhoneNumber"));
    }

    [Fact]
    public void User_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.FirstName.Should().Be(string.Empty);
        user.LastName.Should().Be(string.Empty);
        user.Bio.Should().Be(string.Empty);
        user.Goals.Should().Be(string.Empty);
        user.SocialMediaLinks.Should().NotBeNull().And.BeEmpty();
        user.UserExpertise.Should().NotBeNull().And.BeEmpty();
        user.MentorshipsAsMentor.Should().NotBeNull().And.BeEmpty();
        user.MentorshipsAsMentee.Should().NotBeNull().And.BeEmpty();
        user.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        user.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}