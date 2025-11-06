using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class MentoringManager: IMentoringManager
{
    private readonly IMentoringDataStore _dataStore;
    private readonly ILogger<MentoringManager> _logger;

    public MentoringManager(IMentoringDataStore dataStore, ILogger<MentoringManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
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
        return await _dataStore.CreateMentorshipRequestAsync(mentorship, expertiseIds);   
    }

    public async Task<Mentorship?> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null)
    {
        return await _dataStore.RespondToRequestAsync(mentorshipId, userId, accepted, message);
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
        // Might want to add a cancellation reason here
        // Might want log the cancellation here
        return await _dataStore.CancelMentorshipRequestAsync(mentorshipId, userId);
    }
    public async Task<bool> CompleteMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        // Might want log the completion here
        return await _dataStore.CompleteMentorshipRequestAsync(mentorshipId, userId);
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

    public async Task<Mentorship?> RequestMentorshipWithDetailsAsync(Guid requesterId, Guid targetId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency)
    {
        return await _dataStore.RequestMentorshipWithDetailsAsync(requesterId, targetId, type, requestMessage, focusAreaIds, preferredFrequency);
    }

    public async Task<Mentorship?> GetMentorshipWithRelationships(Guid mentorshipId)
    {
        return await _dataStore.GetMentorshipWithRelationships(mentorshipId);
    }
}