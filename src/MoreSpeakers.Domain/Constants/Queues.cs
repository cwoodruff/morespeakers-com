namespace MoreSpeakers.Domain.Constants;

/// <summary>
/// Contains the names of the queues used by the application.
/// </summary>
public static class Queues
{
    /// <summary>
    /// The queue for sending emails.
    /// </summary>
    public const string SendEmail = "send-email";
    /// <summary>
    /// The poison queue for sending emails.
    /// </summary>
    public const string SendEmailPoison = "send-email-poison";
    /// <summary>
    /// This queue is used for handling ACS EmailDeliveryReportReceived events
    /// </summary>
    public const string BouncedEmails = "bounced-emails";
}