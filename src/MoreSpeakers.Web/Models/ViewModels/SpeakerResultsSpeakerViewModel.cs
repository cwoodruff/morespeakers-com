namespace MoreSpeakers.Web.Models.ViewModels;

public class SpeakerResultsSpeakerViewModel
{
    public required Domain.Models.User Speaker { get; set; }
    public Domain.Models.User? CurrentUser { get; set; }
}