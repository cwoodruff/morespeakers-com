namespace MoreSpeakers.Web.Models.ViewModels;

public class RequestNotificationBadgeViewModel
{
    public RequestNotificationDirection NotificationDirection { get; set; }
    public int Count { get; set; }
}

public enum RequestNotificationDirection
{
    Inbound,
    Outbound
}