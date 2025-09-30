namespace morespeakers.Models;

public class UserExpertise
{
    public Guid UserId { get; set; }
    public int ExpertiseId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Expertise Expertise { get; set; } = null!;
}