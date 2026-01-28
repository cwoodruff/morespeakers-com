namespace MoreSpeakers.Web.Pages.Mentorship;

public partial class RequestsModel
{
    [LoggerMessage(LogLevel.Error, "Error loading mentorship requests for user '{User}'")]
    partial void LogErrorLoadingMentorshipRequests(Exception exception, string? user);

    [LoggerMessage(LogLevel.Error, "Error loading mentorship request for user '{User}'")]
    partial void LogErrorLoadingMentorshipRequest(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Could not find mentorship with ID {MentorshipId}")]
    partial void LogCouldNotFindMentorship(Guid mentorshipId);

    [LoggerMessage(LogLevel.Error, "Could not find mentorship with ID {MentorshipId}")]
    partial void LogCouldNotFindMentorship(Exception exception, Guid mentorshipId);

    [LoggerMessage(LogLevel.Error, "Error accepting mentorship request for user '{User}'")]
    partial void LogErrorAcceptingMentorshipRequest(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship accepted email to mentee")]
    partial void LogFailedToSendMentorshipAcceptedEmailToMentee();

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship accepted email to mentor")]
    partial void LogFailedToSendMentorshipAcceptedEmailToMentor();

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship declined email to mentee")]
    partial void LogFailedToSendMentorshipDeclinedEmailToMentee();

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship declined email to mentor")]
    partial void LogFailedToSendMentorshipDeclinedEmailToMentor();

    [LoggerMessage(LogLevel.Error, "Error declining mentorship request for user '{User}'")]
    partial void LogErrorDecliningMentorshipRequestForUser(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Error loading notification count for user '{User}'")]
    partial void LogErrorLoadingNotificationCountForUser(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Error polling for the incoming request for user '{User}'")]
    partial void LogErrorPollingForTheIncomingRequest(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Error polling for the outbound request for user '{User}'")]
    partial void LogErrorPollingForTheOutboundRequest(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Error cancelling mentorship request for user '{User}'")]
    partial void LogErrorCancellingMentorshipRequest(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship cancelled email to mentee")]
    partial void LogFailedToSendMentorshipCancelledEmailToMentee();

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship cancelled email to mentor")]
    partial void LogFailedToSendMentorshipCancelledEmailToMentor();
}