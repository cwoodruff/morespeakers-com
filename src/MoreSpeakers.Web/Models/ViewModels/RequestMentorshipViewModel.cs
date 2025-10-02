namespace morespeakers.Models.ViewModels;

public class RequestMentorshipViewModel
{
    public User TargetUser { get; set; } = null!;
    public User CurrentUser { get; set; } = null!;
    public MentorshipType Type { get; set; }
    public List<Expertise> AvailableExpertise { get; set; } = new();

    public string GetPlaceholderText()
    {
        return Type == MentorshipType.NewToExperienced
            ? "Hi! I'm interested in learning from your experience. I would love your guidance on..."
            : "Hi! I'd like to connect as peers. I think we could learn from each other in areas like...";
    }

    public string GetRequestButtonText()
    {
        return Type == MentorshipType.NewToExperienced
            ? "Send Mentorship Request"
            : "Send Connection Request";
    }
}
