using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

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
    
    // Passkey support
    Task<IdentityResult> AddOrUpdatePasskeyAsync(User user, UserPasskeyInfo passkey);
    Task<IEnumerable<UserPasskey>> GetUserPasskeysAsync(Guid userId);
    Task<bool> RemovePasskeyAsync(Guid userId, string credentialIdBase64);
    
    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    Task<User?> GetAsync(Guid primaryKey);
    Task<bool> DeleteAsync(Guid primaryKey);
    Task<User> SaveAsync(User entity);
    Task<List<User>> GetAllAsync();
    Task<bool> DeleteAsync(User entity);
    Task<IEnumerable<User>> GetNewSpeakersAsync();
    Task<IEnumerable<User>> GetExperiencedSpeakersAsync();
    Task<SpeakerSearchResult> SearchSpeakersAsync(string? searchTerm, int? speakerTypeId = null, int? expertiseId = null, SpeakerSearchOrderBy sortOrder = SpeakerSearchOrderBy.Name, int? page = null, int? pageSize = null);
    Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId);
    Task<bool> AddUserSocialMediaSiteAsync(Guid userId, UserSocialMediaSite userSocialMediaSite);
    Task<bool> RemoveUserSocialMediaSiteAsync(int userSocialMediaSiteId);
    Task<IEnumerable<UserSocialMediaSite>> GetUserSocialMediaSitesAsync(Guid userId);
    Task<bool> AddExpertiseToUserAsync(Guid userId, int expertiseId);
    Task<bool> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId);
    Task<IEnumerable<UserExpertise>> GetUserExpertisesForUserAsync(Guid userId);
    Task<(int newSpeakers, int experiencedSpeakers, int activeMentorships)> GetStatisticsForApplicationAsync();
    Task<IEnumerable<User>> GetFeaturedSpeakersAsync(int count);
    Task<IEnumerable<SpeakerType>> GetSpeakerTypesAsync();

    // ------------------------------------------
    // Admin Users (List/Search)
    // ------------------------------------------
    Task<PagedResult<UserListRow>> AdminSearchUsersAsync(UserAdminFilter filter, UserAdminSort sort, int page, int pageSize);
    Task<IReadOnlyList<string>> GetAllRoleNamesAsync();
    Task<IReadOnlyList<string>> GetRolesForUserAsync(Guid userId);

    // ------------------------------------------
    // Admin Users (Lock/Unlock)
    // ------------------------------------------
    Task<bool> EnableLockoutAsync(Guid userId, bool enabled);
    Task<bool> SetLockoutEndAsync(Guid userId, DateTimeOffset? lockoutEndUtc);
    Task<bool> UnlockAsync(Guid userId);
    Task<int> GetUserCountInRoleAsync(string roleName);
}