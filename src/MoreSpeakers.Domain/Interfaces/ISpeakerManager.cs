using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISpeakerManager
{
    public Task<User> GetAsync(Guid primaryKey);
    public Task<bool> DeleteAsync(Guid primaryKey);
    public Task<User> SaveAsync(User entity);
    public Task<List<User>> GetAllAsync();
    public Task<bool> DeleteAsync(User entity);
    
    Task<IEnumerable<User>> GetNewSpeakersAsync();
    Task<IEnumerable<User>> GetExperiencedSpeakersAsync();
    Task<IEnumerable<User>> SearchSpeakersAsync(string searchTerm, int? speakerTypeId = null);
    Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId);
    Task<bool> AddSocialMediaLinkAsync(Guid userId, string platform, string url);
    Task<bool> RemoveSocialMediaLinkAsync(int socialMediaId);
    Task<bool> AddExpertiseToUserAsync(Guid userId, int expertiseId);
    Task<bool> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId);
    Task<bool> EmptyAndAddExpertiseForUserAsync (Guid userId, int[] expertises);
    Task<bool> EmptyAndAddSocialMediaForUserAsync(Guid userId, List<SocialMedia> socialMedias);
    Task<List<UserExpertise>> GetUserExpertisesForUserAsync(Guid userId);
    Task<List<SocialMedia>> GetUserSocialMediaForUserAsync(Guid userId);
    Task<(int newSpeakers, int experiencedSpeakers, int activeMentorships)> GetStatisticsForApplicationAsync();
    Task<List<User>> GetFeaturedSpeakersAsync(int count);

}