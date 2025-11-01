namespace MoreSpeakers.Domain.Models;

public class MentorshipExpertise
{
    public Guid MentorshipId { get; set; }
    public int ExpertiseId { get; set; }

    // Navigation properties
    public Mentorship Mentorship { get; set; } = null!;
    public Expertise Expertise { get; set; } = null!;
}