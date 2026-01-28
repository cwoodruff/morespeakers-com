namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public partial class ForgotPasswordModel
{
    [LoggerMessage(LogLevel.Error, "Failed to send password reset email to {Email}")]
    partial void LogFailedToSendPasswordResetEmail(string email);
}