using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Data;

public partial class UserDataStore
{
    [LoggerMessage(LogLevel.Error, "Could not find user with id: {UserId} in the Identity database")]
    partial void LogCouldNotFindUserWithIdUseridInTheIdentityDatabase(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to change password for user with id: {UserId}")]
    partial void LogFailedToChangePasswordForUserWithIdUserid(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to change password for user with id: {UserId}")]
    partial void LogFailedToChangePasswordForUserWithIdUserid(Exception exception, Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to create user with id: {UserId}")]
    partial void LogFailedToCreateUserWithIdUserid(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to create user with id: {UserId}")]
    partial void LogFailedToCreateUserWithIdUserid(Exception exception, Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to add roles to user {UserId}")]
    partial void LogFailedToAddRolesToUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to remove roles from user {UserId}")]
    partial void LogFailedToRemoveRolesFromUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] EnableLockout failed: user {UserId} not found")]
    partial void LogAdminlockoutEnablelockoutFailedUserUseridNotFound(Guid userId);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] SetLockoutEnabledAsync failed for {UserId}: {Errors}")]
    partial void LogAdminlockoutSetlockoutenabledasyncFailedForUseridErrors(Guid userId, string errors);

    [LoggerMessage(LogLevel.Information, "[AdminLockout] LockoutEnabled set to {Enabled} for user {UserId}")]
    partial void LogAdminlockoutLockoutenabledSetToEnabledForUserUserid(bool enabled, Guid userId);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] EnableLockoutAsync exception for user {UserId}")]
    partial void LogAdminlockoutEnablelockoutasyncExceptionForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] EnableLockoutAsync exception for user {UserId}")]
    partial void LogAdminlockoutEnablelockoutasyncExceptionForUserUserid(Exception exception, Guid userId);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] SetLockoutEnd failed: user {UserId} not found")]
    partial void LogAdminlockoutSetlockoutendFailedUserUseridNotFound(Guid userId);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] Rejected SetLockoutEnd with past/now value for {UserId}: {LockoutEnd}")]
    partial void LogAdminlockoutRejectedSetlockoutendWithPastNowValueForUseridLockoutend(Guid userId, DateTimeOffset? lockoutEnd);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] SetLockoutEndDateAsync failed for {UserId}: {Errors}")]
    partial void LogAdminlockoutSetlockoutenddateasyncFailedForUseridErrors(Guid userId, string errors);

    [LoggerMessage(LogLevel.Information, "[AdminLockout] LockoutEnd set to {LockoutEnd} for user {UserId}")]
    partial void LogAdminlockoutLockoutendSetToLockoutendForUserUserid(DateTimeOffset? lockoutEnd, Guid userId);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] SetLockoutEndAsync exception for user {UserId}")]
    partial void LogAdminlockoutSetlockoutendasyncExceptionForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] SetLockoutEndAsync exception for user {UserId}")]
    partial void LogAdminlockoutSetlockoutendasyncExceptionForUserUserid(Exception exception, Guid userId);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] Unlock failed: user {UserId} not found")]
    partial void LogAdminlockoutUnlockFailedUserUseridNotFound(Guid userId);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] Clearing LockoutEnd failed for {UserId}: {Errors}")]
    partial void LogAdminlockoutClearingLockoutendFailedForUseridErrors(Guid userId, string errors);

    [LoggerMessage(LogLevel.Warning, "[AdminLockout] ResetAccessFailedCount failed for {UserId}: {Errors}")]
    partial void LogAdminlockoutResetaccessfailedcountFailedForUseridErrors(Guid userId, string errors);

    [LoggerMessage(LogLevel.Information, "[AdminLockout] User {UserId} unlocked")]
    partial void LogAdminlockoutUserUseridUnlocked(Guid userId);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] UnlockAsync exception for user {UserId}")]
    partial void LogAdminlockoutUnlockasyncExceptionForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] UnlockAsync exception for user {UserId}")]
    partial void LogAdminlockoutUnlockasyncExceptionForUserUserid(Exception exception, Guid userId);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] GetUserCountInRoleAsync failed for role {Role}")]
    partial void LogAdminlockoutGetusercountinroleasyncFailedForRoleRole(string role);

    [LoggerMessage(LogLevel.Error, "[AdminLockout] GetUserCountInRoleAsync failed for role {Role}")]
    partial void LogAdminlockoutGetusercountinroleasyncFailedForRoleRole(Exception exception, string role);

    [LoggerMessage(LogLevel.Error, "Failed to remove passkey for user {UserId}")]
    partial void LogFailedToRemovePasskeyForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to remove passkey for user {UserId}")]
    partial void LogFailedToRemovePasskeyForUserUserid(Exception exception, Guid userId);

    [LoggerMessage(LogLevel.Error, "Failed to add the new user")]
    partial void LogFailedToAddTheNewUser(Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to update the user '{Id}'")]
    partial void LogFailedToUpdateTheUserId(Exception exception, Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to delete the user. Id: '{Id}'")]
    partial void LogFailedToDeleteTheUserIdId(Exception ex, Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to delete the user. Id: '{Id}'")]
    partial void LogFailedToDeleteTheUserIdId(Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to add social media link for user for id: {UserId}. SocialMediaSiteId: {SocialMediaSiteId}, SocialId: {SocialId}")]
    partial void LogFailedToAddSocialMediaLinkForUserForIdUseridSocialmediasiteidSocialmediasiteid(Exception exception,
        Guid userId, int socialMediaSiteId, string socialId);

    [LoggerMessage(LogLevel.Error, "Failed to add social media link for user for id: {UserId}. SocialMediaSiteId: {SocialMediaSiteId}, SocialId: {SocialId}")]
    partial void LogFailedToAddSocialMediaLinkForUserForIdUseridSocialmediasiteidSocialmediasiteid(
        Guid userId, int socialMediaSiteId, string socialId);

    [LoggerMessage(LogLevel.Error, "Failed to remove social media link with id: {UserSocialMediaSiteId}")]
    partial void LogFailedToRemoveSocialMediaLinkWithIdUsersocialmediasiteid(int userSocialMediaSiteId);

    [LoggerMessage(LogLevel.Error, "Failed to remove social media link with id: {UserSocialMediaSiteId}")]
    partial void LogFailedToRemoveSocialMediaLinkWithIdUsersocialmediasiteid(Exception exception, int userSocialMediaSiteId);

    [LoggerMessage(LogLevel.Error, "Failed to add expertise to user with id: {UserId}. ExpertiseId: {ExpertiseId}")]
    partial void LogFailedToAddExpertiseToUserWithIdUseridExpertiseidExpertiseid(Guid userId, int expertiseId);

    [LoggerMessage(LogLevel.Error, "Failed to add expertise to user with id: {UserId}. ExpertiseId: {ExpertiseId}")]
    partial void LogFailedToAddExpertiseToUserWithIdUseridExpertiseidExpertiseid(Exception ex, Guid userId, int expertiseId);

    [LoggerMessage(LogLevel.Error, "Failed to remove expertise from user with id: {UserId}. ExpertiseId: {ExpertiseId}")]
    partial void LogFailedToRemoveExpertiseFromUserWithIdUseridExpertiseidExpertiseid(Guid userId, int expertiseId);

    [LoggerMessage(LogLevel.Error, "Failed to remove expertise from user with id: {UserId}. ExpertiseId: {ExpertiseId}")]
    partial void LogFailedToRemoveExpertiseFromUserWithIdUseridExpertiseidExpertiseid(Exception exception, Guid userId, int expertiseId);
}