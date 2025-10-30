namespace MoreSpeakers.Web.Models.ViewModels;

public class MentorshipRequestsViewModel
{
    public List<Domain.Models.Mentorship> IncomingRequests { get; set; } = new();
    public List<Domain.Models.Mentorship> OutgoingRequests { get; set; } = new();
    public int UnreadCount { get; set; } = 0;
}