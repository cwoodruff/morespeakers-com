using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Managers;

public partial class UserManager
{
    [LoggerMessage(LogLevel.Error, "Failed to remove passkey for user {UserId}")]
    partial void LogFailedToRemovePasskey(Exception exception, Guid userId);

    [LoggerMessage(LogLevel.Warning, "EnableLockoutAsync called with empty user id")]
    partial void LogEnableLockoutCalledWithEmptyUserId();

    [LoggerMessage(LogLevel.Warning, "SetLockoutEndAsync called with empty user id")]
    partial void LogSetLockoutEndCalledWithEmptyUserId();

    [LoggerMessage(LogLevel.Warning, "UnlockAsync called with empty user id")]
    partial void LogUnlockCalledWithEmptyUserId();
}