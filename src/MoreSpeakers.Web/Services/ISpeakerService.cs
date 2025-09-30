using morespeakers.Models;

namespace morespeakers.Services;

// Speaker Service Interface
public interface ISpeakerService
{
    Task<IEnumerable<User>> GetNewSpeakersAsync();
    Task<IEnumerable<User>> GetExperiencedSpeakersAsync();
    Task<User?> GetSpeakerByIdAsync(Guid id);
    Task<IEnumerable<User>> SearchSpeakersAsync(string searchTerm, int? speakerTypeId = null);
    Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId);
    Task<bool> UpdateSpeakerProfileAsync(User user);
    Task<bool> AddSocialMediaLinkAsync(Guid userId, string platform, string url);
    Task<bool> RemoveSocialMediaLinkAsync(int socialMediaId);
    Task<bool> AddExpertiseToUserAsync(Guid userId, int expertiseId);
    Task<bool> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId);
}