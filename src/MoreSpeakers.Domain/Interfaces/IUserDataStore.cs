using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface IUserDataStore
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
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
    
    // Passkey support
    Task<IdentityResult> AddOrUpdatePasskeyAsync(User user, UserPasskeyInfo passkey);
    Task<Result<IEnumerable<UserPasskey>>> GetUserPasskeysAsync(Guid userId);
    Task<Result> RemovePasskeyAsync(Guid userId, byte[] credentialId);

    // ------------------------------------------
    // CRUD Methods (previously inherited from IDataStorePrimaryKeyGuid<User>)
    // ------------------------------------------
    Task<Result<User>> GetAsync(Guid primaryKey);
    Task<Result> DeleteAsync(Guid primaryKey);
    Task<Result<User>> SaveAsync(User entity);
    Task<Result<List<User>>> GetAllAsync();
    Task<Result> DeleteAsync(User entity);

    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    Task<Result<IEnumerable<User>>> GetNewSpeakersAsync();
    Task<Result<IEnumerable<User>>> GetExperiencedSpeakersAsync();
    Task<Result<SpeakerSearchResult>> SearchSpeakersAsync(string? searchTerm, int? speakerTypeId = null, List<int>? expertiseIds = null, SpeakerSearchOrderBy sortOrder = SpeakerSearchOrderBy.Name, int? page = null, int? pageSize = null);
    Task<Result<IEnumerable<User>>> GetSpeakersByExpertiseAsync(int expertiseId);
    Task<Result> AddUserSocialMediaSiteAsync(Guid userId, UserSocialMediaSite userSocialMediaSite);
    Task<Result> RemoveUserSocialMediaSiteAsync(int userSocialMediaSiteId);
    Task<Result<IEnumerable<UserSocialMediaSite>>> GetUserSocialMediaSitesAsync(Guid userId);
    Task<Result> AddExpertiseToUserAsync(Guid userId, int expertiseId);
    Task<Result> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId);
    Task<Result<IEnumerable<UserExpertise>>> GetUserExpertisesForUserAsync(Guid userId);
    Task<Result<(int newSpeakers, int experiencedSpeakers, int activeMentorships)>> GetStatisticsForApplicationAsync();
    Task<Result<IEnumerable<User>>> GetFeaturedSpeakersAsync(int count);
    Task<Result<IEnumerable<SpeakerType>>> GetSpeakerTypesAsync();

    // ------------------------------------------
    // Admin Users (List/Search)
    // ------------------------------------------
    Task<Result<PagedResult<UserListRow>>> AdminSearchUsersAsync(UserAdminFilter filter, UserAdminSort sort, int page, int pageSize);
    Task<Result<IReadOnlyList<string>>> GetAllRoleNamesAsync();
    Task<Result<IReadOnlyList<string>>> GetRolesForUserAsync(Guid userId);
    Task<IdentityResult> AddToRolesAsync(Guid userId, IEnumerable<string> roles);
    Task<IdentityResult> RemoveFromRolesAsync(Guid userId, IEnumerable<string> roles);

    // ------------------------------------------
    // Admin Users (Lock/Unlock)
    // ------------------------------------------
    Task<Result> EnableLockoutAsync(Guid userId, bool enabled);
    Task<Result> SetLockoutEndAsync(Guid userId, DateTimeOffset? lockoutEndUtc);
    Task<Result> UnlockAsync(Guid userId);
    Task<Result<int>> GetUserCountInRoleAsync(string roleName);

    // ------------------------------------------
    // Admin Users (Soft/Hard Delete)
    // ------------------------------------------
    Task<Result> SoftDeleteAsync(Guid userId);
    Task<Result> RestoreAsync(Guid userId);
}