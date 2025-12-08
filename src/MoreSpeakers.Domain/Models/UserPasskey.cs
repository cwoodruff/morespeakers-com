namespace MoreSpeakers.Domain.Models;

/// <summary>
/// A Data Transfer Object representing a user's passkey credential.
/// This is used to display passkey information in the UI without exposing 
/// implementation details like the public key or raw credential ID bytes.
/// </summary>
public class UserPasskey
{
    /// <summary>
    /// The Base64Url encoded Credential ID. 
    /// This ID is safe to use in URLs and HTML attributes.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The User ID this passkey belongs to.
    /// </summary>
    public Guid UserId { get; set; } = Guid.Empty;

    /// <summary>
    /// A user-friendly name for the passkey (e.g., "Chrome on Windows").
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;
}