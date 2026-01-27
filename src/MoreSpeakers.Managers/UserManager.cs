using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Managers;

public class UserManager: IUserManager
{
    private readonly IUserDataStore _dataStore;
    private readonly IOpenGraphSpeakerProfileImageGenerator _openGraphSpeakerProfileImageGenerator;
    private readonly ILogger<UserManager> _logger;

    public UserManager(IUserDataStore dataStore, IOpenGraphSpeakerProfileImageGenerator openGraphSpeakerProfileImageGenerator, ILogger<UserManager> logger)
    {
        _dataStore = dataStore;
        _openGraphSpeakerProfileImageGenerator = openGraphSpeakerProfileImageGenerator;
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
        var result = await _dataStore.CreateAsync(user, password);
        if (result.Succeeded && !string.IsNullOrWhiteSpace(user.HeadshotUrl))
        {
            await _openGraphSpeakerProfileImageGenerator.QueueSpeakerOpenGraphProfileImageCreation(user.Id,
                user.HeadshotUrl, user.FullName);
        }
        return result;
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

    public async Task<bool> ConfirmEmailAsync(User user, string token)
    {
        var result = await _dataStore.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await _dataStore.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        return await _dataStore.ResetPasswordAsync(user, token, newPassword);
    }

    // Passkey Support
    public async Task<IdentityResult> AddOrUpdatePasskeyAsync(User user, UserPasskeyInfo passkey)
    {
        return await _dataStore.AddOrUpdatePasskeyAsync(user, passkey);
    }

    public async Task<IEnumerable<UserPasskey>> GetUserPasskeysAsync(Guid userId)
    {
        return await _dataStore.GetUserPasskeysAsync(userId);
    }

    public async Task<bool> RemovePasskeyAsync(Guid userId, string credentialIdBase64)
    {
        try
        {
            var credentialIdBytes = WebEncoders.Base64UrlDecode(credentialIdBase64);
            return await _dataStore.RemovePasskeyAsync(userId, credentialIdBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove passkey for user {UserId}", userId);
            return false;
        }
    }

    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    public async Task<User?> GetAsync(Guid primaryKey)
    {
        return await _dataStore.GetAsync(primaryKey);
    }

    public async Task<bool> DeleteAsync(Guid primaryKey)
    {
        return await _dataStore.DeleteAsync(primaryKey);
    }

    public async Task<User> SaveAsync(User entity)
    {
        var savedUser = await _dataStore.SaveAsync(entity);
        if (!string.IsNullOrEmpty(savedUser.HeadshotUrl))
        {
            await _openGraphSpeakerProfileImageGenerator.QueueSpeakerOpenGraphProfileImageCreation(savedUser.Id,
                savedUser.HeadshotUrl, savedUser.FullName);
        }
        return savedUser;
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

    public async Task<SpeakerSearchResult> SearchSpeakersAsync(string? searchTerm, int? speakerTypeId = null, List<int>? expertiseIds = null, SpeakerSearchOrderBy sortOrder = SpeakerSearchOrderBy.Name, int? page = null, int? pageSize = null)
    {
        return await _dataStore.SearchSpeakersAsync(searchTerm, speakerTypeId, expertiseIds, sortOrder, page, pageSize);
    }

    public async Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId)
    {
        return await _dataStore.GetSpeakersByExpertiseAsync(expertiseId);
    }

    public async Task<bool> AddUserSocialMediaSiteAsync(Guid userId, UserSocialMediaSite userSocialMediaSite)
    {
        return await _dataStore.AddUserSocialMediaSiteAsync(userId, userSocialMediaSite);
    }

    public async Task<bool> RemoveUserSocialMediaSiteAsync(int userSocialMediaSiteId)
    {
        return await _dataStore.RemoveUserSocialMediaSiteAsync(userSocialMediaSiteId);
    }

    public async Task<IEnumerable<UserSocialMediaSite>> GetUserSocialMediaSitesAsync(Guid userId)
    {
        return userId == Guid.Empty
            ? throw new ArgumentException("Invalid user id")
            : await _dataStore.GetUserSocialMediaSitesAsync(userId);
    }
    
    public async Task<bool> AddExpertiseToUserAsync(Guid userId, int expertiseId)
    {
        return await _dataStore.AddExpertiseToUserAsync(userId, expertiseId);  
    }

    public async Task<bool> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId)
    {
        return await _dataStore.RemoveExpertiseFromUserAsync(userId, expertiseId);   
    }

    public async Task<IEnumerable<UserExpertise>> GetUserExpertisesForUserAsync(Guid userId)
    {
        return await _dataStore.GetUserExpertisesForUserAsync(userId);
    }

    public async Task<(int newSpeakers, int experiencedSpeakers, int activeMentorships)> GetStatisticsForApplicationAsync()
    {
        return await _dataStore.GetStatisticsForApplicationAsync();
    }

    public async Task<IEnumerable<User>> GetFeaturedSpeakersAsync(int count)
    {
        return await _dataStore.GetFeaturedSpeakersAsync(count);
    }

    public async Task<IEnumerable<SpeakerType>> GetSpeakerTypesAsync()
    {
        return await _dataStore.GetSpeakerTypesAsync();
    }

    // ------------------------------------------
    // Admin Users (List/Search)
    // ------------------------------------------

    public async Task<PagedResult<UserListRow>> AdminSearchUsersAsync(UserAdminFilter filter, UserAdminSort sort, int page, int pageSize)
    {
        return await _dataStore.AdminSearchUsersAsync(filter, sort, page, pageSize);
    }

    public async Task<IReadOnlyList<string>> GetAllRoleNamesAsync()
    {
        return await _dataStore.GetAllRoleNamesAsync();
    }

    public async Task<IReadOnlyList<string>> GetRolesForUserAsync(Guid userId)
    {
        return await _dataStore.GetRolesForUserAsync(userId);
    }

    public async Task<IdentityResult> AddToRolesAsync(Guid userId, IEnumerable<string> roles)
    {
        return await _dataStore.AddToRolesAsync(userId, roles);
    }

    public async Task<IdentityResult> RemoveFromRolesAsync(Guid userId, IEnumerable<string> roles)
    {
        return await _dataStore.RemoveFromRolesAsync(userId, roles);
    }

    // ------------------------------------------
    // Admin Users (Lock/Unlock)
    // ------------------------------------------
    public async Task<bool> EnableLockoutAsync(Guid userId, bool enabled)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("EnableLockoutAsync called with empty user id");
            return false;
        }
        return await _dataStore.EnableLockoutAsync(userId, enabled);
    }

    public async Task<bool> SetLockoutEndAsync(Guid userId, DateTimeOffset? lockoutEndUtc)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("SetLockoutEndAsync called with empty user id");
            return false;
        }
        return await _dataStore.SetLockoutEndAsync(userId, lockoutEndUtc);
    }

    public async Task<bool> UnlockAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("UnlockAsync called with empty user id");
            return false;
        }
        return await _dataStore.UnlockAsync(userId);
    }

    public async Task<int> GetUserCountInRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return 0;
        }
        return await _dataStore.GetUserCountInRoleAsync(roleName.Trim());
    }

    // ------------------------------------------
    // Admin Users (Soft/Hard Delete)
    // ------------------------------------------
    public async Task<bool> SoftDeleteAsync(Guid userId)
    {
        if (userId == Guid.Empty) return false;
        var ok = await _dataStore.SoftDeleteAsync(userId);
        if (ok)
        {
            _logger.LogInformation("[AdminAudit] User {UserId} was soft-deleted", userId);
        }
        return ok;
    }

    public async Task<bool> RestoreAsync(Guid userId)
    {
        if (userId == Guid.Empty) return false;
        var ok = await _dataStore.RestoreAsync(userId);
        if (ok)
        {
            _logger.LogInformation("[AdminAudit] User {UserId} was restored", userId);
        }
        return ok;
    }

    public async Task<bool> HardDeleteAsync(Guid userId)
    {
        if (userId == Guid.Empty) return false;
        var ok = await _dataStore.HardDeleteAsync(userId);
        if (ok)
        {
            _logger.LogInformation("[AdminAudit] User {UserId} was HARD-DELETED", userId);
        }
        return ok;
    }
}