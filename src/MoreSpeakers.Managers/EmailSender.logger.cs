using System.Net.Mail;

using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Managers;

public partial class EmailSender
{
    [LoggerMessage(LogLevel.Debug, "Queueing email to {ToAddress} with subject {Subject}")]
    partial void LogQueueingEmail(MailAddress toAddress, string subject);

    [LoggerMessage(LogLevel.Debug, "Sending email to {ToAddress} with subject {Subject}")]
    partial void LogSendingEmail(string toAddress, string subject);

    [LoggerMessage(LogLevel.Debug, "Adding email to Queue. ToAddress: {ToAddress}, Subject: {Subject}")]
    partial void LogAddingEmailToQueue(MailAddress toAddress, string subject);
}