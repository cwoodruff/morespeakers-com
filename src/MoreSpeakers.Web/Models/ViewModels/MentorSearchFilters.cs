namespace MoreSpeakers.Web.Models.ViewModels;

public class MentorSearchFilters
{
    public Domain.Models.MentorshipType Type { get; set; }
    public string? Expertise { get; set; }
    public bool? AvailableNow { get; set; }
}