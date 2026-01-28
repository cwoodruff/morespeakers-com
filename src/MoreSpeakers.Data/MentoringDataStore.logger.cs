using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Data;

public partial class MentoringDataStore
{
    [LoggerMessage(LogLevel.Error, "Failed to save the mentorship. Id: '{Id}'")]
    partial void LogFailedToSaveMentorship(Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to save the mentorship. Id: '{Id}'")]
    partial void LogFailedToSaveMentorship(Exception exception, Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to delete mentorship with id: '{Id}'")]
    partial void LogFailedToDeleteMentorship(Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to delete mentorship with id: '{Id}'")]
    partial void LogFailedToDeleteMentorship(Exception exception, Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request. MentorId: '{MentorId}', MenteeId: '{MenteeId}'")]
    partial void LogFailedToCreateMentorshipRequest(Guid mentorId, Guid menteeid);

    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request. MentorId: '{MentorId}', MenteeId: '{MenteeId}'")]
    partial void LogFailedToCreateMentorshipRequest(Exception exception, Guid mentorid, Guid menteeId);

    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request - Adding expertises. MentorId: '{MentorId}', MenteeId: '{MenteeId}'")]
    partial void LogFailedToCreateMentorshipRequestAddingExpertises(Guid mentorid, Guid menteeid);

    [LoggerMessage(LogLevel.Error, "Failed to respond to mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'")]
    partial void LogFailedToRespondToMentorshipRequest(Guid mentorshipId, Guid userid);

    [LoggerMessage(LogLevel.Error, "Failed to respond to mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'")]
    partial void LogFailedToRespondToMentorshipRequest(Exception exception, Guid mentorshipid, Guid userid);

    [LoggerMessage(LogLevel.Error, "Failed to cancel mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'")]
    partial void LogFailedToCancelMentorshipRequest(Guid mentorshipId, Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to cancel mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'")]
    partial void LogFailedToCancelMentorshipRequest(Exception exception, Guid mentorshipid, Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to complete mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'")]
    partial void LogFailedToCompleteMentorshipRequest(Guid mentorshipid, Guid userid);

    [LoggerMessage(LogLevel.Error, "Failed to complete mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'")]
    partial void LogFailedToCompleteMentorshipRequest(Exception exception, Guid mentorshipid, Guid userid);

    [LoggerMessage(LogLevel.Warning, "User {UserId} already has a mentorship request pending or active with user {TargetId}")]
    partial void LogUserAlreadyHasAMentorshipRequest(Guid userid, Guid targetid);

    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request with details. MentorId: '{MentorId}', MenteeId: '{MenteeId}'")]
    partial void LogFailedToCreateMentorshipRequestWithDetails(Guid mentorid, Guid menteeid);

    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request with details. MentorId: '{MentorId}', MenteeId: '{MenteeId}'")]
    partial void LogFailedToCreateMentorshipRequestWithDetails(Exception exception, Guid mentorid, Guid menteeid);

    [LoggerMessage(LogLevel.Error, "Failed to create mentorship request with details - Save Expertises. MentorId: '{MentorId}', MenteeId: '{MenteeId}'")]
    partial void LogFailedToCreateMentorshipRequestDuringSaveExpertise(Guid mentorid, Guid menteeId);
}