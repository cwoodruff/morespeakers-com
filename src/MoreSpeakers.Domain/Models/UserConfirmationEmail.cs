namespace MoreSpeakers.Domain.Models;

/// <summary>
/// This is the model for the user confirmation email.
/// </summary>
public class UserConfirmationEmail
{
    /// <summary>
    /// The URL to confirm the user.
    /// </summary>
    public required string ConfirmationUrl { get; init; }
    /// <summary>
    /// The user to confirm.
    /// </summary>
    public required User User { get; init; }
}