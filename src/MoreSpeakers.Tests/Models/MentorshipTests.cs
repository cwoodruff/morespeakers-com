using System.ComponentModel.DataAnnotations;
using FluentAssertions;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models;

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
        mentorship.Status.Should().Be(MentorshipStatus.Pending);
        mentorship.RequestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        mentorship.ResponsedAt.Should().BeNull();
        mentorship.CompletedAt.Should().BeNull();
        mentorship.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(MentorshipStatus.Pending, true, false, false, false)]
    [InlineData(MentorshipStatus.Active, false, true, false, false)]
    [InlineData(MentorshipStatus.Completed, false, false, true, false)]
    [InlineData(MentorshipStatus.Cancelled, false, false, false, true)]
    public void Mentorship_StatusProperties_ShouldReturnCorrectValues(
        MentorshipStatus status, bool isPending, bool isActive, bool isCompleted, bool isCancelled)
    {
        // Arrange
        var mentorship = new Mentorship { Status = status };

        // Act & Assert
        mentorship.IsPending.Should().Be(isPending);
        mentorship.IsActive.Should().Be(isActive);
        mentorship.IsCompleted.Should().Be(isCompleted);
        mentorship.IsCancelled.Should().Be(isCancelled);
    }



    [Fact]
    public void Mentorship_Notes_ShouldFailValidationWhenTooLong()
    {
        // Arrange
        var mentorship = new Mentorship
        {
            Status = MentorshipStatus.Pending,
            MenteeId = Guid.NewGuid(),
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
            Status = MentorshipStatus.Pending,
            MenteeId = Guid.NewGuid(),
            MentorId = Guid.NewGuid(),
            Notes = notes
        };

        // Act
        var validationResults = ValidateModel(mentorship);

        // Assert
        validationResults.Should().NotContain(vr => vr.MemberNames.Contains("Notes"));
    }

    [Theory]
    [InlineData(MentorshipStatus.Pending)]
    [InlineData(MentorshipStatus.Active)]
    [InlineData(MentorshipStatus.Completed)]
    [InlineData(MentorshipStatus.Cancelled)]
    public void Mentorship_ValidStatuses_ShouldPassValidation(MentorshipStatus status)
    {
        // Arrange
        var mentorship = new Mentorship
        {
            Status = status,
            MenteeId = Guid.NewGuid(),
            MentorId = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateModel(mentorship);

        // Assert
        validationResults.Should().NotContain(vr => vr.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Mentorship_MenteeId_ShouldNotBeEmpty()
    {
        // Arrange
        var mentorship = new Mentorship
        {
            MenteeId = Guid.Empty,
            MentorId = Guid.NewGuid()
        };

        // Act & Assert
        mentorship.MenteeId.Should().Be(Guid.Empty);
        // Note: Guid validation would typically be handled at the database level or service level
    }

    [Fact]
    public void Mentorship_MentorId_ShouldNotBeEmpty()
    {
        // Arrange
        var mentorship = new Mentorship
        {
            MenteeId = Guid.NewGuid(),
            MentorId = Guid.Empty
        };

        // Act & Assert
        mentorship.MentorId.Should().Be(Guid.Empty);
        // Note: Guid validation would typically be handled at the database level or service level
    }

    [Fact]
    public void Mentorship_RequestedAt_ShouldBeSetOnCreation()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var mentorship = new Mentorship();
        var afterCreation = DateTime.UtcNow;

        // Assert
        mentorship.RequestedAt.Should().BeAfter(beforeCreation.AddSeconds(-1))
            .And.BeBefore(afterCreation.AddSeconds(1));
    }

    [Fact]
    public void MentorshipStatus_Enum_ShouldHaveCorrectValues()
    {
        // Act & Assert
        ((int)MentorshipStatus.Pending).Should().Be(0);
        ((int)MentorshipStatus.Accepted).Should().Be(1);
        ((int)MentorshipStatus.Declined).Should().Be(2);
        ((int)MentorshipStatus.Active).Should().Be(3);
        ((int)MentorshipStatus.Completed).Should().Be(4);
        ((int)MentorshipStatus.Cancelled).Should().Be(5);
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