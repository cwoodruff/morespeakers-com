namespace MoreSpeakers.Domain.Models;

/// <summary>
/// This is the model for the user confirmation email.
/// </summary>
public class UserConfirmationEmail
{
    /// <summary>
    /// The URL to confirm the user.
    /// </summary>
    public string ConfirmationUrl { get; set; } = string.Empty;
    /// <summary>
    /// The user to confirm.
    /// </summary>
    public User User { get; set; }
}