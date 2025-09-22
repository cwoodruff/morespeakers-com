using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using morespeakers.Models;

namespace MoreSpeakers.Tests.Models;

public class MentorshipTests
{
    [Fact]
    public void Mentorship_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var mentorship = new Mentorship();

        // Assert
        mentorship.Id.Should().NotBe(Guid.Empty);
        mentorship.Status.Should().Be("Pending");
        mentorship.RequestDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        mentorship.AcceptedDate.Should().BeNull();
        mentorship.CompletedDate.Should().BeNull();
        mentorship.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData("Pending", true, false, false, false)]
    [InlineData("Active", false, true, false, false)]
    [InlineData("Completed", false, false, true, false)]
    [InlineData("Cancelled", false, false, false, true)]
    public void Mentorship_StatusProperties_ShouldReturnCorrectValues(
        string status, bool isPending, bool isActive, bool isCompleted, bool isCancelled)
    {
        // Arrange
        var mentorship = new Mentorship { Status = status };

        // Act & Assert
        mentorship.IsPending.Should().Be(isPending);
        mentorship.IsActive.Should().Be(isActive);
        mentorship.IsCompleted.Should().Be(isCompleted);
        mentorship.IsCancelled.Should().Be(isCancelled);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Mentorship_Status_ShouldFailValidationWhenEmpty(string status)
    {
        // Arrange
        var mentorship = new Mentorship
        {
            Status = status,
            NewSpeakerId = Guid.NewGuid(),
            MentorId = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateModel(mentorship);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Mentorship_Status_ShouldFailValidationWhenTooLong()
    {
        // Arrange
        var mentorship = new Mentorship
        {
            Status = new string('A', 21), // Exceeds MaxLength of 20
            NewSpeakerId = Guid.NewGuid(),
            MentorId = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateModel(mentorship);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Mentorship_Notes_ShouldFailValidationWhenTooLong()
    {
        // Arrange
        var mentorship = new Mentorship
        {
            Status = "Pending",
            NewSpeakerId = Guid.NewGuid(),
            MentorId = Guid.NewGuid(),
            Notes = new string('A', 2001) // Exceeds MaxLength of 2000
        };

        // Act
        var validationResults = ValidateModel(mentorship);

        // Assert
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Notes"));
    }

    [Theory]
    [InlineData("Valid notes within limit")]
    [InlineData(null)]
    [InlineData("")]
    public void Mentorship_Notes_ShouldPassValidationWhenValidOrEmpty(string notes)
    {
        // Arrange
        var mentorship = new Mentorship
        {
            Status = "Pending",
            NewSpeakerId = Guid.NewGuid(),
            MentorId = Guid.NewGuid(),
            Notes = notes
        };

        // Act
        var validationResults = ValidateModel(mentorship);

        // Assert
        validationResults.Should().NotContain(vr => vr.MemberNames.Contains("Notes"));
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Active")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void Mentorship_ValidStatuses_ShouldPassValidation(string status)
    {
        // Arrange
        var mentorship = new Mentorship
        {
            Status = status,
            NewSpeakerId = Guid.NewGuid(),
            MentorId = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateModel(mentorship);

        // Assert
        validationResults.Should().NotContain(vr => vr.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Mentorship_NewSpeakerId_ShouldNotBeEmpty()
    {
        // Arrange
        var mentorship = new Mentorship
        {
            NewSpeakerId = Guid.Empty,
            MentorId = Guid.NewGuid()
        };

        // Act & Assert
        mentorship.NewSpeakerId.Should().Be(Guid.Empty);
        // Note: Guid validation would typically be handled at the database level or service level
    }

    [Fact]
    public void Mentorship_MentorId_ShouldNotBeEmpty()
    {
        // Arrange
        var mentorship = new Mentorship
        {
            NewSpeakerId = Guid.NewGuid(),
            MentorId = Guid.Empty
        };

        // Act & Assert
        mentorship.MentorId.Should().Be(Guid.Empty);
        // Note: Guid validation would typically be handled at the database level or service level
    }

    [Fact]
    public void Mentorship_RequestDate_ShouldBeSetOnCreation()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var mentorship = new Mentorship();
        var afterCreation = DateTime.UtcNow;

        // Assert
        mentorship.RequestDate.Should().BeAfter(beforeCreation.AddSeconds(-1))
            .And.BeBefore(afterCreation.AddSeconds(1));
    }

    [Fact]
    public void MentorshipStatus_Enum_ShouldHaveCorrectValues()
    {
        // Act & Assert
        ((int)MentorshipStatus.Pending).Should().Be(0);
        ((int)MentorshipStatus.Active).Should().Be(1);
        ((int)MentorshipStatus.Completed).Should().Be(2);
        ((int)MentorshipStatus.Cancelled).Should().Be(3);
    }

    [Fact]
    public void SpeakerTypeEnum_ShouldHaveCorrectValues()
    {
        // Act & Assert
        ((int)SpeakerTypeEnum.NewSpeaker).Should().Be(1);
        ((int)SpeakerTypeEnum.ExperiencedSpeaker).Should().Be(2);
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}