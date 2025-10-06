using MoreSpeakers.Web.Models;

namespace MoreSpeakers.Web.Services;


// Mentorship Service Interface
public interface IMentorshipService
{
    Task<IEnumerable<Mentorship>> GetMentorshipsForMentorAsync(Guid mentorId);
    Task<IEnumerable<Mentorship>> GetMentorshipsForNewSpeakerAsync(Guid newSpeakerId);
    Task<Mentorship?> GetMentorshipByIdAsync(Guid id);
    Task<bool> RequestMentorshipAsync(Guid newSpeakerId, Guid mentorId, string? notes = null);
    Task<bool> AcceptMentorshipAsync(Guid mentorshipId);
    Task<bool> CompleteMentorshipAsync(Guid mentorshipId, string? notes = null);
    Task<bool> CancelMentorshipAsync(Guid mentorshipId, string? reason = null);
    Task<bool> UpdateMentorshipNotesAsync(Guid mentorshipId, string notes);
    Task<IEnumerable<Mentorship>> GetPendingMentorshipsAsync();
    Task<IEnumerable<Mentorship>> GetActiveMentorshipsAsync();

    // Enhanced methods for full connection flow
    Task<Mentorship?> RequestMentorshipWithDetailsAsync(Guid requesterId, Guid targetId, MentorshipType type,
        string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency);
    Task<bool> DeclineMentorshipAsync(Guid mentorshipId, string? declineReason);
    Task<(int incoming, int outgoing)> GetPendingCountsAsync(Guid userId);
    Task<IEnumerable<Mentorship>> GetMentorshipsForUserAsync(Guid userId, MentorshipStatus? status = null);
    Task<bool> CanRequestMentorshipAsync(Guid requesterId, Guid targetId);
    Task<IEnumerable<Mentorship>> GetIncomingRequestsAsync(Guid userId);
    Task<IEnumerable<Mentorship>> GetOutgoingRequestsAsync(Guid userId);
}