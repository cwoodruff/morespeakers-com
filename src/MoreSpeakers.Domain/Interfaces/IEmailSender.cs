using System.Net.Mail;

namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Represents a service for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Queues up an email to send
    /// </summary>
    /// <param name="toAddress">The to address of the email</param>
    /// <param name="subject">The subject of the email</param>
    /// <param name="body">The body of the email</param>
    Task QueueEmail(MailAddress toAddress, string subject, string body);
    
    /// <summary>
    /// Queues up an email to send
    /// </summary>
    /// <param name="toAddress">The to address of the email</param>
    /// <param name="subject">The subject of the email</param>
    /// <param name="body">The body of the email</param>
    /// <param name="fromAddress">The from address of the email</param>
    /// <param name="replyToAddress">The email address to reply to</param>
    Task QueueEmail(MailAddress toAddress,
        string subject, string body, MailAddress fromAddress , MailAddress replyToAddress);
}