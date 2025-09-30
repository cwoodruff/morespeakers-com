namespace morespeakers.Models.ViewModels;

public class MentorshipRequestModel
{
    public string RequestMessage { get; set; } = string.Empty;
    public string? PreferredFrequency { get; set; }
    public List<int> SelectedExpertiseIds { get; set; } = new();
}