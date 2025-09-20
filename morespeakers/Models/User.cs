using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace morespeakers.Models;

public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public override string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(6000)] // Approximately 1000 words
    public string Bio { get; set; } = string.Empty;

    [Url]
    [MaxLength(500)]
    public string? SessionizeUrl { get; set; }

    [MaxLength(500)]
    public string? HeadshotUrl { get; set; }

    [Required]
    public int SpeakerTypeId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Goals { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public SpeakerType SpeakerType { get; set; } = null!;
    public ICollection<SocialMedia> SocialMediaLinks { get; set; } = new List<SocialMedia>();
    public ICollection<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public ICollection<Mentorship> MentorshipsAsMentor { get; set; } = new List<Mentorship>();
    public ICollection<Mentorship> MentorshipsAsNewSpeaker { get; set; } = new List<Mentorship>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public bool IsNewSpeaker => SpeakerType?.Name == "NewSpeaker";
    public bool IsExperiencedSpeaker => SpeakerType?.Name == "ExperiencedSpeaker";
}