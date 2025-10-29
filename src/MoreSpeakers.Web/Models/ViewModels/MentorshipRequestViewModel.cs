namespace MoreSpeakers.Web.Models.ViewModels;

public class MentorshipRequestViewModel
{
    public Domain.Models.User Mentor { get; set; } = null!;
    public Domain.Models.User Mentee { get; set; } = null!;
    public Domain.Models.MentorshipType MentorshipType { get; set; }
    public List<Domain.Models.Expertise> AvailableExpertise { get; set; } = new();
    public string Message { get; set; } = string.Empty;

    public string GetPlaceholderText()
    {
        return MentorshipType == Domain.Models.MentorshipType.NewToExperienced
            ? "Hi! I'm interested in learning from your experience. I would love your guidance on..."
            : "Hi! I'd like to connect as peers. I think we could learn from each other in areas like...";
    }

    public string GetRequestButtonText()
    {
        return MentorshipType == Domain.Models.MentorshipType.NewToExperienced
            ? "Send Mentorship Request"
            : "Send Connection Request";
    }
}