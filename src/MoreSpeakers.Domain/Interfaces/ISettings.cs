using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Settings for the application.
/// </summary>
public interface ISettings
{
    /// <summary>
    /// Gets or sets the Azure Storage Blob connection settings.
    /// </summary>
    public string AzureBlobStorageConnectionString { get; init; }
    
    /// <summary>
    /// Gets or sets the Azure Storage Table connection settings.
    /// </summary>
    public string AzureTableStorageConnectionString { get; init; }
    
    /// <summary>
    /// Gets or sets the Azure Storage Queue connection settings.
    /// </summary>
    public string AzureQueueStorageConnectionString { get; init; }
    
    /// <summary>
    /// The email settings.
    /// </summary>
    public EmailSettings Email { get; init; }
}