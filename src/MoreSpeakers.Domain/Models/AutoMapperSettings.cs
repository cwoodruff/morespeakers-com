namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Settings for AutoMapper.
/// </summary>
public class AutoMapperSettings: IAutoMapperSettings
{
    /// <summary>
    /// The license key for AutoMapper.
    /// </summary>
    public string LicenseKey { get; init; } = string.Empty;
}