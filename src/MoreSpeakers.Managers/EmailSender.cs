using System.Net.Mail;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.Messages;
using JosephGuadagno.AzureHelpers.Storage;

namespace MoreSpeakers.Managers;

/// <summary>
/// Sends emails
/// </summary>
public class EmailSender: IEmailSender, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ISettings _settings;
    private readonly ILogger<EmailSender> _logger;

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="queueServiceClient">Azure Queue Service client</param>
    /// <param name="settings">The <see cref="MoreSpeakers.Domain.Models.Settings"/></param>
    /// <param name="logger">The logger</param>
    public EmailSender(QueueServiceClient queueServiceClient, ISettings settings, ILogger<EmailSender> logger)
    {
        _queueServiceClient = queueServiceClient;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Queues up an email to send
    /// </summary>
    /// <param name="toAddress">The to address of the email</param>
    /// <param name="subject">The subject of the email</param>
    /// <param name="body">The body of the email</param>
    public async Task QueueEmail(MailAddress toAddress, string subject, string body)
    {
        var fromAddress = new MailAddress(_settings.Email.FromAddress, _settings.Email.FromName);
        var replyToAddress = new MailAddress(_settings.Email.ReplyToAddress, _settings.Email.ReplyToName);
        await QueueEmail(toAddress, subject, body, fromAddress, replyToAddress);
    }
    
    /// <summary>
    /// Queues up an email to send
    /// </summary>
    /// <param name="toAddress">The to address of the email</param>
    /// <param name="subject">The subject of the email</param>
    /// <param name="body">The body of the email</param>
    /// <param name="fromAddress">The from address of the email</param>
    /// <param name="replyToAddress">The email address to reply to</param>
    public async Task QueueEmail(MailAddress toAddress,
        string subject, string body, MailAddress fromAddress , MailAddress replyToAddress) 
    {
        var emailMessage = new Email
        {
            Body = body,
            FromDisplayName = fromAddress.DisplayName,
            FromMailAddress = fromAddress.Address,
            Subject = subject,
            ToDisplayName = toAddress.DisplayName,
            ToMailAddress = toAddress.Address,
            ReplyToMailAddress = replyToAddress.Address,
            ReplyToDisplayName = replyToAddress.DisplayName
        };

        var queue = new Queue(_queueServiceClient, Domain.Constants.Queues.SendEmail);
        await queue.AddMessageAsync(emailMessage);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await QueueEmail(new MailAddress(email), subject, htmlMessage);
    }
}