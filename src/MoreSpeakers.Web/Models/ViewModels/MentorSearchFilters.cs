namespace MoreSpeakers.Web.Models.ViewModels;

public class MentorSearchFilters
{
    public MentorshipType Type { get; set; }
    public string? Expertise { get; set; }
    public bool? AvailableNow { get; set; }
}