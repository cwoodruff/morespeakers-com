namespace MoreSpeakers.Web.Pages.Mentorship;

public partial class ActiveModel
{
    [LoggerMessage(LogLevel.Error, "Failed to load active mentorships for user '{User}'")]
    partial void LogFailedToLoadMentorships(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Failed to complete mentorship request for user '{User}'")]
    partial void LogFailedToCompleteMentorshipRequest(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Failed to cancel mentorship request for user '{User}'")]
    partial void LogFailedToCancelMentorshipRequest(Exception exception, Guid? user);
}