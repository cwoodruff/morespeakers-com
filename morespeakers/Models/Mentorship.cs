using System.ComponentModel.DataAnnotations;

namespace morespeakers.Models;

public class Mentorship
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid NewSpeakerId { get; set; }
    public Guid MentorId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Navigation properties
    public User NewSpeaker { get; set; } = null!;
    public User Mentor { get; set; } = null!;

    // Computed properties
    public bool IsPending => Status == "Pending";
    public bool IsActive => Status == "Active";
    public bool IsCompleted => Status == "Completed";
    public bool IsCancelled => Status == "Cancelled";
}

// Enums for strongly typed values
public enum MentorshipStatus
{
    Pending,
    Active,
    Completed,
    Cancelled
}

public enum SpeakerTypeEnum
{
    NewSpeaker = 1,
    ExperiencedSpeaker = 2
}