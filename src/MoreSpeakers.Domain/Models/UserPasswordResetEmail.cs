namespace MoreSpeakers.Domain.Models;

/// <summary>
/// This is the model for the user reset email.
/// </summary>
public class UserPasswordResetEmail
{
    /// <summary>
    /// The URL to reset the user's email.
    /// </summary>
    public string? ResetEmailUrl { get; set; } = string.Empty;
    /// <summary>
    /// The user to confirm.
    /// </summary>
    public User User { get; set; }
}