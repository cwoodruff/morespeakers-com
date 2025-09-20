using morespeakers.Models;

namespace morespeakers.Services;

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
}