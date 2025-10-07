using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Settings for the application.
/// </summary>
public interface ISettings
{
    /// <summary>
    /// Gets or sets the Azure Storage connection settings.
    /// </summary>
    public string AzureStorageConnectionString { get; init; }
    
    /// <summary>
    /// The email settings.
    /// </summary>
    public EmailSettings Email { get; init; }
}