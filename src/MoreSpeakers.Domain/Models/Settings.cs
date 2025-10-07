using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Domain.Models;

/// <summary>
/// Settings for the application.
/// </summary>
public class Settings:ISettings
{
    /// <summary>
    /// Gets or sets the Azure Storage connection settings.
    /// </summary>
    public required string AzureStorageConnectionString { get; init; }
    
    /// <summary>
    /// The email settings.
    /// </summary>
    public required EmailSettings Email { get; init; }
}