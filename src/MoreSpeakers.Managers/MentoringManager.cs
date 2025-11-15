using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Constants;
using MoreSpeakers.Domain.Extensions;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class MentoringManager: IMentoringManager
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
    
    public async Task<Mentorship> GetAsync(Guid primaryKey)
    {
        return await _dataStore.GetAsync(primaryKey);
    }

    public async Task<bool> DeleteAsync(Guid primaryKey)
    {
        return await _dataStore.DeleteAsync(primaryKey);
    }

    public async Task<Mentorship> SaveAsync(Mentorship entity)
    {
        return await _dataStore.SaveAsync(entity);
    }

    public async Task<List<Mentorship>> GetAllAsync()
    {
        return await _dataStore.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(Mentorship entity)
    {
        return await _dataStore.DeleteAsync(entity);
    }

    public async Task<List<Expertise>> GetSharedExpertisesAsync(User mentor, User mentee)
    {
        return await _dataStore.GetSharedExpertisesAsync(mentor, mentee);
    }

    public async Task<bool> DoesMentorshipRequestsExistsAsync(User mentor, User mentee)
    {
        return await _dataStore.DoesMentorshipRequestsExistsAsync(mentor, mentee);
    }

    public async Task<bool> CreateMentorshipRequestAsync(Mentorship mentorship, List<int> expertiseIds)
    {
        var result =  await _dataStore.CreateMentorshipRequestAsync(mentorship, expertiseIds);

        if (result)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipRequested,
                new Dictionary<string, string> { { "MentorId", mentorship.MentorId.ToString() } });    
        }
        else
        {
            _logger.LogError("Failed to create mentorship request for mentor '{MentorId}'", mentorship.MentorId);
        }
        return result;
    }
    
    public async Task<Mentorship?> RequestMentorshipWithDetailsAsync(Guid menteeId, Guid mentorId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency)
    {
        var result = await _dataStore.RequestMentorshipWithDetailsAsync(menteeId, mentorId, type, requestMessage, focusAreaIds, preferredFrequency);

        if (result is not null)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipRequestedWithDetails,
                new Dictionary<string, string>
                {
                    { "MenteeId", menteeId.ToString() },
                    { "MentorId", mentorId.ToString() },
                    { "Type", type.GetDescription() },
                    { "RequestMessage", requestMessage ?? "No request message provided." },
                });  
        }
        else
        {
            _logger.LogError(
                "Failed to create mentorship request with details for mentee '{MenteeId}' and mentor '{MentorId}'",
                menteeId, mentorId);
        }
        return result;
    }

    public async Task<Mentorship?> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null)
    {
        var result = await _dataStore.RespondToRequestAsync(mentorshipId, userId, accepted, message);

        if (result is not null)
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
            _logger.LogError("Failed to respond to mentorship request '{MentorshipId}' for user '{User}'", mentorshipId, userId);
        }
        return result;
    }

    public async Task<List<Mentorship>> GetActiveMentorshipsForUserAsync(Guid userId)
    {
        return await _dataStore.GetActiveMentorshipsForUserAsync(userId);
    }

    public async Task<(int outboundCount, int inboundCount)> GetNumberOfMentorshipsPending(Guid userId)
    {
        return await _dataStore.GetNumberOfMentorshipsPending(userId);
    }

    public async Task<List<Mentorship>> GetIncomingMentorshipRequests(Guid userId)
    {
        return await _dataStore.GetIncomingMentorshipRequests(userId);
    }

    public async Task<List<Mentorship>> GetOutgoingMentorshipRequests(Guid userId)
    {
        return await _dataStore.GetOutgoingMentorshipRequests(userId);
    }

    public async Task<bool> CancelMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var result =  await _dataStore.CancelMentorshipRequestAsync(mentorshipId, userId);

        if (result)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipCancelled,
                new Dictionary<string, string>
                {
                    { "MentorshipId", mentorshipId.ToString() }, { "UserId", userId.ToString() }
                });    
        }
        else
        {
            _logger.LogError("Failed to cancel mentorship request '{MentorshipId}' for user '{User}'", mentorshipId, userId);
        }
        
        return result;
    }
    public async Task<bool> CompleteMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var result = await _dataStore.CompleteMentorshipRequestAsync(mentorshipId, userId);

        if (result)
        {
            _telemetryClient.TrackEvent(TelemetryEvents.ManagerEvents.MentorshipCompleted,
                new Dictionary<string, string>
                {
                    { "MentorshipId", mentorshipId.ToString() }, { "UserId", userId.ToString() }
                });
        }
        else
        {
            _logger.LogError("Failed to complete mentorship request '{MentorshipId}' for user '{User}'", mentorshipId, userId);
        }
        return result;
    }

    public async Task<List<User>> GetMentorsExceptForUserAsync(Guid userId, MentorshipType mentorshipType, List<string>? expertiseNames, bool? availability = true)
    {
        return await _dataStore.GetMentorsExceptForUserAsync(userId, mentorshipType, expertiseNames, availability);
    }

    public async Task<User?> GetMentorAsync(Guid userId)
    {
        return await _dataStore.GetMentorAsync(userId);   
    }

    public async Task<bool> CanRequestMentorshipAsync(Guid menteeId, Guid mentorId)
    {
        return await _dataStore.CanRequestMentorshipAsync(menteeId, mentorId);
    }

    public async Task<Mentorship?> GetMentorshipWithRelationships(Guid mentorshipId)
    {
        return await _dataStore.GetMentorshipWithRelationships(mentorshipId);
    }
}