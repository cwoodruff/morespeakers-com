using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Constants;
using MoreSpeakers.Domain.Extensions;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public partial class MentoringManager : IMentoringManager
{
    private readonly IMentoringDataStore _dataStore;
    private readonly ILogger<MentoringManager> _logger;
    private readonly TelemetryClient _telemetryClient;

    public MentoringManager(IMentoringDataStore dataStore, ILogger<MentoringManager> logger, TelemetryClient telemetryClient)
    {
        _dataStore = dataStore;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public Task<Result<Mentorship>> GetAsync(Guid primaryKey) => _dataStore.GetAsync(primaryKey);

    public Task<Result> DeleteAsync(Guid primaryKey) => _dataStore.DeleteAsync(primaryKey);

    public Task<Result<Mentorship>> SaveAsync(Mentorship entity) => _dataStore.SaveAsync(entity);

    public Task<Result<List<Mentorship>>> GetAllAsync() => _dataStore.GetAllAsync();

    public Task<Result<List<Expertise>>> GetSharedExpertisesAsync(User mentor, User mentee) =>
        _dataStore.GetSharedExpertisesAsync(mentor, mentee);

    public Task<Result<bool>> DoesMentorshipRequestsExistsAsync(User mentor, User mentee) =>
        _dataStore.DoesMentorshipRequestsExistsAsync(mentor, mentee);

    public async Task<Result> CreateMentorshipRequestAsync(Mentorship mentorship, List<int> expertiseIds)
    {
        var result = await _dataStore.CreateMentorshipRequestAsync(mentorship, expertiseIds);
        if (result.IsSuccess)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipRequested,
                new Dictionary<string, string> { { "MentorId", mentorship.MentorId.ToString() } });
        }
        else
        {
            LogFailedToCreateMentorMentorshipRequest(mentorship.MentorId);
        }

        return result;
    }

    public async Task<Result<Mentorship>> RequestMentorshipWithDetailsAsync(Guid menteeId, Guid mentorId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency)
    {
        var result = await _dataStore.RequestMentorshipWithDetailsAsync(
            menteeId,
            mentorId,
            type,
            requestMessage,
            focusAreaIds,
            preferredFrequency);

        if (result.IsSuccess)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipRequestedWithDetails,
                new Dictionary<string, string>
                {
                    { "MenteeId", menteeId.ToString() },
                    { "MentorId", mentorId.ToString() },
                    { "Type", type.GetDescription() },
                    { "RequestMessage", requestMessage ?? "No request message provided." }
                });
        }
        else
        {
            LogFailedToCreateMentorshipRequestWithMenteeDetails(menteeId, mentorId);
        }

        return result;
    }

    public async Task<Result<Mentorship>> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null)
    {
        var result = await _dataStore.RespondToRequestAsync(mentorshipId, userId, accepted, message);
        if (result.IsSuccess)
        {
            _telemetryClient.TrackEvent(
                accepted
                    ? TelemetryEvents.ManagerEvents.MentorshipAccepted
                    : TelemetryEvents.ManagerEvents.MentorshipDeclined,
                new Dictionary<string, string>
                {
                    { "MentorshipId", mentorshipId.ToString() },
                    { "UserId", userId.ToString() },
                    { "Reason", message ?? "No reason provided." }
                });
        }
        else
        {
            LogFailedToRespondToMentorshipRequest(mentorshipId, userId);
        }

        return result;
    }

    public Task<Result<List<Mentorship>>> GetActiveMentorshipsForUserAsync(Guid userId) =>
        _dataStore.GetActiveMentorshipsForUserAsync(userId);

    public Task<Result<(int outboundCount, int inboundCount)>> GetNumberOfMentorshipsPending(Guid userId) =>
        _dataStore.GetNumberOfMentorshipsPending(userId);

    public Task<Result<List<Mentorship>>> GetIncomingMentorshipRequests(Guid userId) =>
        _dataStore.GetIncomingMentorshipRequests(userId);

    public Task<Result<List<Mentorship>>> GetOutgoingMentorshipRequests(Guid userId) =>
        _dataStore.GetOutgoingMentorshipRequests(userId);

    public async Task<Result> CancelMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var result = await _dataStore.CancelMentorshipRequestAsync(mentorshipId, userId);
        if (result.IsSuccess)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipCancelled,
                new Dictionary<string, string>
                {
                    { "MentorshipId", mentorshipId.ToString() },
                    { "UserId", userId.ToString() }
                });
        }
        else
        {
            LogFailedToCancelMentorshipRequest(mentorshipId, userId);
        }

        return result;
    }

    public async Task<Result> CompleteMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var result = await _dataStore.CompleteMentorshipRequestAsync(mentorshipId, userId);
        if (result.IsSuccess)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipCompleted,
                new Dictionary<string, string>
                {
                    { "MentorshipId", mentorshipId.ToString() },
                    { "UserId", userId.ToString() }
                });
        }
        else
        {
            LogFailedToCompleteMentorshipRequest(mentorshipId, userId);
        }

        return result;
    }

    public Task<Result<List<User>>> GetMentorsExceptForUserAsync(Guid userId, MentorshipType mentorshipType, List<string>? expertiseNames, bool? availability = true) =>
        _dataStore.GetMentorsExceptForUserAsync(userId, mentorshipType, expertiseNames, availability);

    public Task<Result<User>> GetMentorAsync(Guid userId) => _dataStore.GetMentorAsync(userId);

    public Task<Result<bool>> CanRequestMentorshipAsync(Guid menteeId, Guid mentorId) =>
        _dataStore.CanRequestMentorshipAsync(menteeId, mentorId);

    public Task<Result<Mentorship>> GetMentorshipWithRelationships(Guid mentorshipId) =>
        _dataStore.GetMentorshipWithRelationships(mentorshipId);
}
