namespace MoreSpeakers.Web.Models.ViewModels;

public class MentorshipRequestsViewModel
{
    public List<Domain.Models.Mentorship> IncomingRequests { get; set; } = [];
    public List<Domain.Models.Mentorship> OutgoingRequests { get; set; } = [];
    public int UnreadCount { get; set; } = 0;
}