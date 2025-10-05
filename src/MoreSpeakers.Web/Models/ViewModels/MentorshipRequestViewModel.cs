namespace MoreSpeakers.Web.Models.ViewModels;

public class MentorshipRequestViewModel
{
    public User Mentor { get; set; } = null!;
    public MentorshipType Type { get; set; }
    public MentorshipType MentorshipType { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Expertise> SharedExpertise { get; set; } = new();
}