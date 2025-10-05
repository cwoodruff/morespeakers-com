using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Web.Models;

public class Mentorship
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MentorId { get; set; }
    public Guid MenteeId { get; set; }

    public MentorshipStatus Status { get; set; } = MentorshipStatus.Pending;
    public MentorshipType Type { get; set; } = MentorshipType.NewToExperienced;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResponsedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(2000)] public string? RequestMessage { get; set; }
    [MaxLength(2000)] public string? ResponseMessage { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
    [MaxLength(100)] public string? PreferredFrequency { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Mentor { get; set; } = null!;
    public User Mentee { get; set; } = null!;
    public ICollection<MentorshipExpertise> FocusAreas { get; set; } = new List<MentorshipExpertise>();

    // Computed properties
    public bool IsPending => Status == MentorshipStatus.Pending;
    public bool IsActive => Status == MentorshipStatus.Active;
    public bool IsCompleted => Status == MentorshipStatus.Completed;
    public bool IsCancelled => Status == MentorshipStatus.Cancelled;
}