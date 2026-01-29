namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public partial class RegisterModel
{
    [LoggerMessage(LogLevel.Error, "Error submitting new expertise")]
    partial void LogErrorSubmittingNewExpertise(Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to save new user account")]
    partial void LogFailedToSaveNewUserAccount(Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to find user after saving registration. Email: '{Email}'")]
    partial void LogFailedToFindUser(string email);

    [LoggerMessage(LogLevel.Error, "Failed to save the user's expertise's and social media sites. Email: '{Email}'")]
    partial void LogFailedToSaveExpertiseSocialMediaSites(Exception exception, string email);

    [LoggerMessage(LogLevel.Error, "Failed to send the welcome email")]
    partial void LogFailedToSendTheWelcomeEmail();

    [LoggerMessage(LogLevel.Warning, "Failed to generate confirmation link for user {UserId}")]
    partial void LogFailedToGenerateConfirmationLink(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to send email confirmation email to user {UserId}")]
    partial void LogFailedToSendConfirmationEmail(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to add a social media row")]
    partial void LogFailedToAddASocialMediaRow(Exception exception);

    [LoggerMessage(LogLevel.Information, "User created a new account with password")]
    partial void LogUserCreatedNewAccount();
}