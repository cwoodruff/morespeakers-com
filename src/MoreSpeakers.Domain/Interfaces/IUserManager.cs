using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IUserManager
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
    Task<bool> ConfirmEmailAsync(User user, string token);
    
    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    public Task<User?> GetAsync(Guid primaryKey);
    public Task<bool> DeleteAsync(Guid primaryKey);
    public Task<User> SaveAsync(User entity);
    public Task<List<User>> GetAllAsync();
    public Task<bool> DeleteAsync(User entity);
    Task<IEnumerable<User>> GetNewSpeakersAsync();
    Task<IEnumerable<User>> GetExperiencedSpeakersAsync();
    Task<SpeakerSearchResult> SearchSpeakersAsync(string? searchTerm, int? speakerTypeId = null, int? expertiseId = null, SpeakerSearchOrderBy sortOrder = SpeakerSearchOrderBy.Name, int? page = null, int? pageSize = null);
    Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId);
    Task<bool> AddUserSocialMediaSiteAsync(Guid userId, UserSocialMediaSite userSocialMediaSite);
    Task<bool> RemoveUserSocialMediaSiteAsync(int userSocialMediaSiteId);
    Task<bool> EmptyAndAddUserSocialMediaSiteForUserAsync(Guid userId, List<UserSocialMediaSite> userSocialMediaSites);
    Task<IEnumerable<UserSocialMediaSite>> GetUserSocialMediaSitesAsync(Guid userId);
    Task<bool> AddExpertiseToUserAsync(Guid userId, int expertiseId);
    Task<bool> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId);
    Task<bool> EmptyAndAddExpertiseForUserAsync (Guid userId, int[] expertises);
    Task<IEnumerable<UserExpertise>> GetUserExpertisesForUserAsync(Guid userId);
    Task<(int newSpeakers, int experiencedSpeakers, int activeMentorships)> GetStatisticsForApplicationAsync();
    Task<IEnumerable<User>> GetFeaturedSpeakersAsync(int count);
    Task<IEnumerable<SpeakerType>> GetSpeakerTypesAsync();

}