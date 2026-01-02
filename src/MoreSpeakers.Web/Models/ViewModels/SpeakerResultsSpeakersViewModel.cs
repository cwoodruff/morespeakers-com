namespace MoreSpeakers.Web.Models.ViewModels;

public class SpeakerResultsSpeakersViewModel
{
    public required IEnumerable<Domain.Models.User> Speakers { get; set; }
    public Domain.Models.User? CurrentUser { get; set; }
}