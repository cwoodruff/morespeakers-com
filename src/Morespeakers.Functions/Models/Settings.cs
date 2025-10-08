using MoreSpeakers.Functions.Interfaces;

namespace MoreSpeakers.Functions.Models;

/// <summary>
/// The settings for the function application
/// </summary>
public class Settings: ISettings
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
    /// Gets or sets the Azure Communication Services connection settings.
    /// </summary>
    public required string AzureCommunicationsConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the email statuses that are considered bounced.
    /// </summary>
    public required string BouncedEmailStatuses { get; set; }
}