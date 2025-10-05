namespace MoreSpeakers.Functions.Interfaces;

/// <summary>
/// The settings for the function application
/// </summary>
public interface ISettings
{
    /// <summary>
    /// Gets or sets the Azure Storage connection settings.
    /// </summary>
    public string AzureStorageConnectionString { get; set; }
    
    /// <summary>
    /// Gets or sets the Azure Communication Services connection settings.
    /// </summary>
    public string AzureCommunicationsConnectionString { get; set; }
    
    /// <summary>
    /// Gets or sets the email statuses that are considered bounced.
    /// </summary>
    public string BouncedEmailStatuses { get; set; }
}