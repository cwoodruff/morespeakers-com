using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Managers;

public partial class MentoringManager
{
    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request for mentor '{MentorId}'")]
    partial void LogFailedToCreateMentorMentorshipRequest(Guid mentorId);

    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request with details for mentee '{MenteeId}' and mentor '{MentorId}'")]
    partial void LogFailedToCreateMentorshipRequestWithMenteeDetails(Guid menteeId, Guid mentorId);

    [LoggerMessage(LogLevel.Error, "Failed to respond to mentorship request '{MentorshipId}' for user '{User}'")]
    partial void LogFailedToRespondToMentorshipRequest(Guid mentorshipId, Guid user);

    [LoggerMessage(LogLevel.Error, "Failed to cancel mentorship request '{MentorshipId}' for user '{User}'")]
    partial void LogFailedToCancelMentorshipRequest(Guid mentorshipId, Guid user);

    [LoggerMessage(LogLevel.Error, "Failed to complete mentorship request '{MentorshipId}' for user '{User}'")]
    partial void LogFailedToCompleteMentorshipRequest(Guid mentorshipId, Guid user);
}