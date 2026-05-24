using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Managers;

public partial class UserManager: IUserManager
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

    public async Task<Result> ConfirmEmailAsync(User user, string token)
    {
        var identityResult = await _dataStore.ConfirmEmailAsync(user, token);
        return identityResult.Succeeded
            ? Result.Success()
            : Result.Failure(new Error("user.confirm-email.failed",
                string.Join("; ", identityResult.Errors.Select(e => e.Description))));
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

    public async Task<Result<IEnumerable<UserPasskey>>> GetUserPasskeysAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Result.Failure<IEnumerable<UserPasskey>>(new Error("user.validation.user-id-invalid", "User ID is required."));
        return await _dataStore.GetUserPasskeysAsync(userId);
    }

    public async Task<Result> RemovePasskeyAsync(Guid userId, string credentialIdBase64)
    {
        if (string.IsNullOrWhiteSpace(credentialIdBase64))
            return Result.Failure(new Error("user.validation.credential-id-required", "Credential ID is required."));
        try
        {
            var credentialIdBytes = WebEncoders.Base64UrlDecode(credentialIdBase64);
            return await _dataStore.RemovePasskeyAsync(userId, credentialIdBytes);
        }
        catch (FormatException)
        {
            return Result.Failure(new Error("user.validation.credential-id-invalid", "Invalid credential ID format."));
        }
    }

    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    public Task<Result<User>> GetAsync(Guid primaryKey) => _dataStore.GetAsync(primaryKey);

    public Task<Result> DeleteAsync(Guid primaryKey) => _dataStore.DeleteAsync(primaryKey);

    public async Task<Result<User>> SaveAsync(User entity)
    {
        var result = await _dataStore.SaveAsync(entity);
        if (result.IsSuccess && !string.IsNullOrEmpty(result.Value.HeadshotUrl))
        {
            await _openGraphSpeakerProfileImageGenerator.QueueSpeakerOpenGraphProfileImageCreation(
                result.Value.Id, result.Value.HeadshotUrl, result.Value.FullName);
        }
        return result;
    }

    public Task<Result<List<User>>> GetAllAsync() => _dataStore.GetAllAsync();

    public Task<Result> DeleteAsync(User entity) => _dataStore.DeleteAsync(entity);

    public Task<Result<IEnumerable<User>>> GetNewSpeakersAsync() => _dataStore.GetNewSpeakersAsync();

    public Task<Result<IEnumerable<User>>> GetExperiencedSpeakersAsync() => _dataStore.GetExperiencedSpeakersAsync();

    public Task<Result<SpeakerSearchResult>> SearchSpeakersAsync(string? searchTerm, int? speakerTypeId = null, List<int>? expertiseIds = null, SpeakerSearchOrderBy sortOrder = SpeakerSearchOrderBy.Name, int? page = null, int? pageSize = null)
        => _dataStore.SearchSpeakersAsync(searchTerm, speakerTypeId, expertiseIds, sortOrder, page, pageSize);

    public Task<Result<IEnumerable<User>>> GetSpeakersByExpertiseAsync(int expertiseId)
        => _dataStore.GetSpeakersByExpertiseAsync(expertiseId);

    public async Task<Result> AddUserSocialMediaSiteAsync(Guid userId, UserSocialMediaSite userSocialMediaSite)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        if (userSocialMediaSite == null)
            return Result.Failure(new Error("user.validation.social-media-site-required", "Social media site is required."));
        return await _dataStore.AddUserSocialMediaSiteAsync(userId, userSocialMediaSite);
    }

    public async Task<Result> RemoveUserSocialMediaSiteAsync(int userSocialMediaSiteId)
    {
        if (userSocialMediaSiteId <= 0)
            return Result.Failure(new Error("user.validation.social-media-site-id-invalid", "Social media site ID is invalid."));
        return await _dataStore.RemoveUserSocialMediaSiteAsync(userSocialMediaSiteId);
    }

    public async Task<Result<IEnumerable<UserSocialMediaSite>>> GetUserSocialMediaSitesAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Result.Failure<IEnumerable<UserSocialMediaSite>>(new Error("user.validation.user-id-invalid", "User ID is required."));
        return await _dataStore.GetUserSocialMediaSitesAsync(userId);
    }
    
    public async Task<Result> AddExpertiseToUserAsync(Guid userId, int expertiseId)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        if (expertiseId <= 0)
            return Result.Failure(new Error("user.validation.expertise-id-invalid", "Expertise ID is invalid."));
        return await _dataStore.AddExpertiseToUserAsync(userId, expertiseId);
    }

    public async Task<Result> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        if (expertiseId <= 0)
            return Result.Failure(new Error("user.validation.expertise-id-invalid", "Expertise ID is invalid."));
        return await _dataStore.RemoveExpertiseFromUserAsync(userId, expertiseId);
    }

    public Task<Result<IEnumerable<UserExpertise>>> GetUserExpertisesForUserAsync(Guid userId)
        => _dataStore.GetUserExpertisesForUserAsync(userId);

    public Task<Result<(int newSpeakers, int experiencedSpeakers, int activeMentorships)>> GetStatisticsForApplicationAsync()
        => _dataStore.GetStatisticsForApplicationAsync();

    public Task<Result<IEnumerable<User>>> GetFeaturedSpeakersAsync(int count)
        => _dataStore.GetFeaturedSpeakersAsync(count);

    public Task<Result<IEnumerable<SpeakerType>>> GetSpeakerTypesAsync()
        => _dataStore.GetSpeakerTypesAsync();

    // ------------------------------------------
    // Admin Users (List/Search)
    // ------------------------------------------

    public Task<Result<PagedResult<UserListRow>>> AdminSearchUsersAsync(UserAdminFilter filter, UserAdminSort sort, int page, int pageSize)
        => _dataStore.AdminSearchUsersAsync(filter, sort, page, pageSize);

    public Task<Result<IReadOnlyList<string>>> GetAllRoleNamesAsync()
        => _dataStore.GetAllRoleNamesAsync();

    public Task<Result<IReadOnlyList<string>>> GetRolesForUserAsync(Guid userId)
        => _dataStore.GetRolesForUserAsync(userId);

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
    public async Task<Result> EnableLockoutAsync(Guid userId, bool enabled)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        return await _dataStore.EnableLockoutAsync(userId, enabled);
    }

    public async Task<Result> SetLockoutEndAsync(Guid userId, DateTimeOffset? lockoutEndUtc)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        return await _dataStore.SetLockoutEndAsync(userId, lockoutEndUtc);
    }

    public async Task<Result> UnlockAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        return await _dataStore.UnlockAsync(userId);
    }

    public async Task<Result<int>> GetUserCountInRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return Result.Failure<int>(new Error("user.validation.role-name-required", "Role name is required."));
        return await _dataStore.GetUserCountInRoleAsync(roleName.Trim());
    }

    // ------------------------------------------
    // Admin Users (Soft/Hard Delete)
    // ------------------------------------------
    public async Task<Result> SoftDeleteAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        var result = await _dataStore.SoftDeleteAsync(userId);
        if (result.IsSuccess)
            _logger.LogInformation("[AdminAudit] User {UserId} was soft-deleted", userId);
        return result;
    }

    public async Task<Result> RestoreAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Result.Failure(new Error("user.validation.user-id-invalid", "User ID is required."));
        var result = await _dataStore.RestoreAsync(userId);
        if (result.IsSuccess)
            _logger.LogInformation("[AdminAudit] User {UserId} was restored", userId);
        return result;
    }
}