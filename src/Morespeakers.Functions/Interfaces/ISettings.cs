namespace MoreSpeakers.Functions.Interfaces;

/// <summary>
/// The settings for the function application
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
    /// Gets or sets the Azure Communication Services connection settings.
    /// </summary>
    public string AzureCommunicationsConnectionString { get; set; }
    
    /// <summary>
    /// Gets or sets the email statuses that are considered bounced.
    /// </summary>
    public string BouncedEmailStatuses { get; set; }
}