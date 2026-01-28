namespace MoreSpeakers.Web.Pages.Speakers;

public partial class IndexModel
{
    [LoggerMessage(LogLevel.Error, "Error loading speakers")]
    partial void LogErrorLoadingSpeakers(Exception exception);

    [LoggerMessage(LogLevel.Error, "Error loading mentor request modal for user '{User}'")]
    partial void LogErrorLoadingMentorRequestModel(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Error sending mentorship request for user '{User}'")]
    partial void LogErrorSendingMentorshipRequest(Exception exception, Guid? user);

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship request email to mentee")]
    partial void LogFailedToSendMentorshipRequestEmailToMentee();

    [LoggerMessage(LogLevel.Error, "Failed to send mentorship request email to mentor")]
    partial void LogFailedToSendMentorshipRequestEmailToMentor();
}