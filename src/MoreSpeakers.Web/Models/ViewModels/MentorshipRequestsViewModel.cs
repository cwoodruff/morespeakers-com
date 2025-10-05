namespace MoreSpeakers.Web.Models.ViewModels;

public class MentorshipRequestsViewModel
{
    public List<Mentorship> IncomingRequests { get; set; } = new();
    public List<Mentorship> OutgoingRequests { get; set; } = new();
    public int UnreadCount { get; set; } = 0;
}