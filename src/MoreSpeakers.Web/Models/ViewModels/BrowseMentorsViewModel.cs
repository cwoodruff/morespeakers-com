namespace MoreSpeakers.Web.Models.ViewModels;

public class BrowseMentorsViewModel
{
    public User CurrentUser { get; set; } = null!;
    public MentorshipType MentorshipType { get; set; } = MentorshipType.NewToExperienced;
    public List<string> SelectedExpertise { get; set; } = new();
    public bool? AvailableNow { get; set; }
    public List<Expertise> AvailableExpertise { get; set; } = new();
    public List<User> Mentors { get; set; } = new();
    
    // Additional properties for backward compatibility
    public List<Expertise> AllExpertise { get; set; } = new();
    public List<Guid> SelectedExpertiseIds { get; set; } = new();
    public MentorshipType Type { get; set; } = MentorshipType.NewToExperienced;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalMentors { get; set; } = 0;
}