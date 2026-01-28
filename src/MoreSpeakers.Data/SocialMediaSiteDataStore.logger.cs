using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Data;

public partial class SocialMediaSiteDataStore
{
    [LoggerMessage(LogLevel.Error, "Failed to save the social media site. Id: '{Id}', Name: '{Name}'")]
    partial void LogFailedToSaveSocialMediaSite(int id, string name);

    [LoggerMessage(LogLevel.Error, "Failed to save the social media site. Id: '{Id}', Name: '{Name}'")]
    partial void LogFailedToSaveSocialMediaSite(Exception exception, int id, string name);

    [LoggerMessage(LogLevel.Error, "Failed to delete social media site with id: '{Id}'")]
    partial void LogFailedToDeleteSocialMediaSite(int id);

    [LoggerMessage(LogLevel.Error, "Failed to delete social media site with id: '{Id}'")]
    partial void LogFailedToDeleteSocialMediaSite(Exception exception, int id);
}