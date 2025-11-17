using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace MoreSpeakers.Domain.Models;

public class User: IdentityUser<Guid>
{
   
    [Required] [MaxLength(100)] public string FirstName { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(6000)] // Approximately 1000 words
    public string Bio { get; set; } = string.Empty;

    [Url] [MaxLength(500)] public string? SessionizeUrl { get; set; }

    [MaxLength(500)] public string? HeadshotUrl { get; set; }

    [Required] public int SpeakerTypeId { get; set; }

    [Required] [MaxLength(2000)] public string Goals { get; set; } = string.Empty;

    // Mentorship preferences
    public bool IsAvailableForMentoring { get; set; } = true;
    public int MaxMentees { get; set; } = 2;
    [MaxLength(1000)] public string? MentorshipFocus { get; set; } // What they want to help with

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public SpeakerType SpeakerType { get; set; } = null!;
    public ICollection<SocialMedia> SocialMediaLinks { get; set; } = new List<SocialMedia>();
    public ICollection<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public ICollection<Mentorship> MentorshipsAsMentor { get; set; } = new List<Mentorship>();
    public ICollection<Mentorship> MentorshipsAsMentee { get; set; } = new List<Mentorship>();
    public ICollection<UserSocialMediaSite> UserSocialMediaSites { get; set; } = new List<UserSocialMediaSite>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public bool IsNewSpeaker => SpeakerType.Id == (int) SpeakerTypeEnum.NewSpeaker;
    public bool IsExperiencedSpeaker => SpeakerType.Id == (int) SpeakerTypeEnum.ExperiencedSpeaker;
}