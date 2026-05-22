using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Managers;

public partial class OpenGraphSpeakerProfileImageGenerator
{
    [LoggerMessage(LogLevel.Information, "Queued OpenGraph profile image creation for speaker {Id}")]
    static partial void LogQueuedOpengraphProfileImageCreationForSpeakerId(ILogger<OpenGraphSpeakerProfileImageGenerator> logger, Guid id);

    [LoggerMessage(LogLevel.Error, "Failed to get image stream from URL {Url}")]
    static partial void LogFailedToGetImageStreamFromUrl(ILogger<OpenGraphSpeakerProfileImageGenerator> logger, string url, Exception exception);
}