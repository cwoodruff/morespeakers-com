namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public partial class LoginModel
{
    [LoggerMessage(LogLevel.Information, "User logged in")]
    partial void LogUserLoggedIn();

    [LoggerMessage(LogLevel.Warning, "User account locked out")]
    partial void LogUserAccountLockedOut();
}