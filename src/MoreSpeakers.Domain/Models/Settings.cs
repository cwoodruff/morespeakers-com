using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Domain.Models;

/// <summary>
/// Settings for the application.
/// </summary>
public class Settings : ISettings
{
    /// <summary>
    /// Gets or sets the Azure Storage Blob connection settings.
    /// </summary>
    public required string AzureBlobStorageConnectionString { get; init; }

    /// <summary>
    /// Gets or sets the Azure Storage Table connection settings.
    /// </summary>
    public required string AzureTableStorageConnectionString { get; init; }
    
    /// <summary>
    /// Gets or sets the Azure Storage Queue connection settings.
    /// </summary>
    public required string AzureQueueStorageConnectionString { get; init; }

    /// <summary>
    /// The email settings.
    /// </summary>
    public required EmailSettings Email { get; init; }
}