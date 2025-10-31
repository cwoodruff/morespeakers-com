using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class UserManager: IUserManager
{
    private readonly IUserDataStore _dataStore;
    private readonly ILogger<UserManager> _logger;

    public UserManager(IUserDataStore dataStore, ILogger<UserManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }
    
    // ------------------------------------------
    // Wrapper methods for AspNetCore Identity
    // ------------------------------------------

    public async Task<User?> GetUserAsync(ClaimsPrincipal user)
    {
        return await _dataStore.GetUserAsync(user);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        return await _dataStore.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        return await _dataStore.CreateAsync(user, password);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _dataStore.FindByEmailAsync(email);
    }

    public async Task<User?> GetUserIdAsync(ClaimsPrincipal user)
    {
        return await _dataStore.GetUserIdAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        return await _dataStore.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> UpdateAsync(User user)
    {
        return await _dataStore.UpdateAsync(user);
    }


    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    
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

    public async Task<IEnumerable<User>> GetNewSpeakersAsync()
    {
        return await _dataStore.GetNewSpeakersAsync();
    }

    public async Task<IEnumerable<User>> GetExperiencedSpeakersAsync()
    {
        return await _dataStore.GetExperiencedSpeakersAsync();
    }


    public async Task<IEnumerable<User>> SearchSpeakersAsync(string searchTerm, int? speakerTypeId = null)
    {
        return await _dataStore.SearchSpeakersAsync(searchTerm, speakerTypeId);
    }

    public async Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId)
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

    public async Task<(int newSpeakers, int experiencedSpeakers, int activeMentorships)> GetStatisticsForApplicationAsync()
    {
        return await _dataStore.GetStatisticsForApplicationAsync();
    }

    public async Task<List<User>> GetFeaturedSpeakersAsync(int count)
    {
        // TODO: Need to improve this logic and just not pick the count with most expertise
        return await _dataStore.GetFeaturedSpeakersAsync(count);
    }
}