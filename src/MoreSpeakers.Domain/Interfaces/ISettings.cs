using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Settings for the application.
/// </summary>
public interface ISettings
{   
    /// <summary>
    /// The email settings.
    /// </summary>
    public EmailSettings Email { get; init; }

    /// <summary>
    /// The GitHub settings.
    /// </summary>
    public GitHubSettings GitHub { get; init; }
    
    /// <summary>
    /// The AutoMapper settings.
    /// </summary>
    public AutoMapperSettings AutoMapper { get; init; }

    /// <summary>
    /// The Application Insights settings.
    /// </summary>
    ApplicationInsightsSettings ApplicationInsights { get; init; }
}