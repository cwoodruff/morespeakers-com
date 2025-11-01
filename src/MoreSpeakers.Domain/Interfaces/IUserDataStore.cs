using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IUserDataStore: IDataStorePrimaryKeyGuid<User>
{
    // ------------------------------------------
    // Wrapper methods for AspNetCore Identity
    // ------------------------------------------
    Task<User?> GetUserAsync(ClaimsPrincipal user);
    Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> GetUserIdAsync(ClaimsPrincipal user);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    
    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
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