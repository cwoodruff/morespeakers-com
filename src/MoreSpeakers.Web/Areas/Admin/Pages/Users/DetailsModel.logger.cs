namespace MoreSpeakers.Web.Areas.Admin.Pages.Users;

public partial class DetailsModel
{
    [LoggerMessage(LogLevel.Error, "Failed to retrieve roles for user {UserId}")]
    partial void LogFailedToRetrieveRolesForUser(Exception exception, Guid userId);
}