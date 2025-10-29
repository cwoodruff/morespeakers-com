using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class SpeakerManager: ISpeakerManager
{
    private readonly ISpeakerDataStore _dataStore;
    private readonly ILogger<SpeakerManager> _logger;

    public SpeakerManager(ISpeakerDataStore dataStore, ILogger<SpeakerManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }
    
    public async Task<User> GetAsync(Guid primaryKey)
    {
        return await _dataStore.GetAsync(primaryKey);
    }

    public async Task<bool> DeleteAsync(Guid primaryKey)
    {
        return await _dataStore.DeleteAsync(primaryKey);
    }

    public async Task<User> SaveAsync(User entity)
    {
        return await _dataStore.SaveAsync(entity);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _dataStore.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(User entity)
    {
        return await _dataStore.DeleteAsync(entity);
    }

    public async Task<IEnumerable<Domain.Models.User>> GetNewSpeakersAsync()
    {
        return await _dataStore.GetNewSpeakersAsync();
    }

    public async Task<IEnumerable<Domain.Models.User>> GetExperiencedSpeakersAsync()
    {
        return await _dataStore.GetExperiencedSpeakersAsync();
    }


    public async Task<IEnumerable<Domain.Models.User>> SearchSpeakersAsync(string searchTerm, int? speakerTypeId = null)
    {
        return await _dataStore.SearchSpeakersAsync(searchTerm, speakerTypeId);
    }

    public async Task<IEnumerable<Domain.Models.User>> GetSpeakersByExpertiseAsync(int expertiseId)
    {
        return await _dataStore.GetSpeakersByExpertiseAsync(expertiseId);
    }

    public async Task<bool> AddSocialMediaLinkAsync(Guid userId, string platform, string url)
    {
        return await _dataStore.AddSocialMediaLinkAsync(userId, platform, url);
    }

    public async Task<bool> RemoveSocialMediaLinkAsync(int socialMediaId)
    {
        return await _dataStore.RemoveSocialMediaLinkAsync(socialMediaId); 
    }

    public async Task<bool> AddExpertiseToUserAsync(Guid userId, int expertiseId)
    {
        return await _dataStore.AddExpertiseToUserAsync(userId, expertiseId);  
    }

    public async Task<bool> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId)
    {
        return await _dataStore.RemoveExpertiseFromUserAsync(userId, expertiseId);   
    }

    public async Task<bool> EmptyAndAddExpertiseForUserAsync(Guid userId, int[] expertises)
    {
        if (userId == Guid.Empty || userId == Guid.NewGuid())
        {
            throw new ArgumentException("Invalid user id");
        }
        return await _dataStore.EmptyAndAddExpertiseForUserAsync(userId, expertises); 
    }

    public async Task<bool> EmptyAndAddSocialMediaForUserAsync(Guid userId, List<SocialMedia> socialMedias)
    {
        if (userId == Guid.Empty || userId == Guid.NewGuid())
        {
            throw new ArgumentException("Invalid user id");
        }
        return await _dataStore.EmptyAndAddSocialMediaForUserAsync(userId, socialMedias);
    }

    public async Task<List<UserExpertise>> GetUserExpertisesForUserAsync(Guid userId)
    {
        return await _dataStore.GetUserExpertisesForUserAsync(userId);
    }

    public async Task<List<SocialMedia>> GetUserSocialMediaForUserAsync(Guid userId)
    {
        return await _dataStore.GetUserSocialMediaForUserAsync(userId);
    }
}