using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IMentoringManager
{
    public Task<Mentorship> GetAsync(Guid primaryKey);
    public Task<bool> DeleteAsync(Guid primaryKey);
    public Task<Mentorship> SaveAsync(Mentorship entity);
    public Task<List<Mentorship>> GetAllAsync();
    public Task<bool> DeleteAsync(Mentorship entity);
    public Task<List<Expertise>> GetSharedExpertisesAsync(User mentor, User mentee);
    public Task<bool> DoesMentorshipRequestsExistsAsync(User mentor, User mentee);
    public Task<bool> CreateMentorshipRequestAsync(Mentorship mentorship, List<int> expertiseIds);
    public Task<Mentorship?> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null);
    public Task<List<Mentorship>> GetActiveMentorshipsForUserAsync(Guid userId);
    public Task<(int outboundCount, int inboundCount)> GetNumberOfMentorshipsPending(Guid userId);
    public Task<List<Mentorship>> GetIncomingMentorshipRequests(Guid userId);
    public Task<List<Mentorship>> GetOutgoingMentorshipRequests(Guid userId);
    public Task<bool> CancelMentorshipRequestAsync(Guid mentorshipId, Guid userId);
    public Task<bool> CompleteMentorshipRequestAsync(Guid mentorshipId, Guid userId);
    public Task<List<User>> GetMentorsExceptForUserAsync(Guid userId, MentorshipType mentorshipType,
        List<string>? expertiseNames, bool? availability = true);
    public Task<User?> GetMentorAsync (Guid userId);
    public Task<bool> CanRequestMentorshipAsync(Guid menteeId, Guid mentorId);
    public Task<Mentorship?> RequestMentorshipWithDetailsAsync(Guid requesterId, Guid targetId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency);
    public Task<Mentorship?> GetMentorshipWithRelationships(Guid mentorshipId);
}