namespace MoreSpeakers.Web.Services;

public partial class TemplatedEmailSender
{
    [LoggerMessage(LogLevel.Information, "{EventName} email was successfully sent to {Email}")]
    partial void LogEmailWasSuccessfullySent(string eventName, string email);

    [LoggerMessage(LogLevel.Error, "Failed to send {EventName} email to {Email}")]
    partial void LogFailedToSendEmail(Exception exception, string eventName, string email);
}