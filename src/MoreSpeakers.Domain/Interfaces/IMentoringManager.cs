using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IMentoringManager
{
    public Task<Result<Mentorship>> GetAsync(Guid primaryKey);
    public Task<Result> DeleteAsync(Guid primaryKey);
    public Task<Result<Mentorship>> SaveAsync(Mentorship entity);
    public Task<Result<List<Mentorship>>> GetAllAsync();
    public Task<Result<List<Expertise>>> GetSharedExpertisesAsync(User mentor, User mentee);
    public Task<Result<bool>> DoesMentorshipRequestsExistsAsync(User mentor, User mentee);
    public Task<Result> CreateMentorshipRequestAsync(Mentorship mentorship, List<int> expertiseIds);
    public Task<Result<Mentorship>> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null);
    public Task<Result<List<Mentorship>>> GetActiveMentorshipsForUserAsync(Guid userId);
    public Task<Result<(int outboundCount, int inboundCount)>> GetNumberOfMentorshipsPending(Guid userId);
    public Task<Result<List<Mentorship>>> GetIncomingMentorshipRequests(Guid userId);
    public Task<Result<List<Mentorship>>> GetOutgoingMentorshipRequests(Guid userId);
    public Task<Result> CancelMentorshipRequestAsync(Guid mentorshipId, Guid userId);
    public Task<Result> CompleteMentorshipRequestAsync(Guid mentorshipId, Guid userId);
    public Task<Result<List<User>>> GetMentorsExceptForUserAsync(Guid userId, MentorshipType mentorshipType,
        List<string>? expertiseNames, bool? availability = true);
    public Task<Result<User>> GetMentorAsync (Guid userId);
    public Task<Result<bool>> CanRequestMentorshipAsync(Guid menteeId, Guid mentorId);
    public Task<Result<Mentorship>> RequestMentorshipWithDetailsAsync(Guid menteeId, Guid userId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency);
    public Task<Result<Mentorship>> GetMentorshipWithRelationships(Guid mentorshipId);
}