namespace MoreSpeakers.Web.Models.ViewModels;

public class BrowseMentorsViewModel
{
    public Domain.Models.User CurrentUser { get; set; } = null!;
    public Domain.Models.MentorshipType MentorshipType { get; set; } = Domain.Models.MentorshipType.NewToExperienced;
    public List<string> SelectedExpertise { get; set; } = [];
    public bool? AvailableNow { get; set; }
    public List<Domain.Models.Expertise> AvailableExpertise { get; set; } = [];
    public List<Domain.Models.User> Mentors { get; set; } = [];

    // Additional properties for backward compatibility
    public List<Domain.Models.Expertise> AllExpertise { get; set; } = [];
    public List<Guid> SelectedExpertiseIds { get; set; } = [];
    public Domain.Models.MentorshipType Type { get; set; } = Domain.Models.MentorshipType.NewToExperienced;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalMentors { get; set; } = 0;
}