using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Managers;

public partial class ExpertiseManager
{
    [LoggerMessage(LogLevel.Information, "Soft-deleted expertise with id {Id}")]
    partial void LogSoftDeletedExpertiseWithIdId(int id);

    [LoggerMessage(LogLevel.Warning, "Soft delete failed for expertise id {Id} with error code {ErrorCode}")]
    partial void LogSoftDeleteFailedForExpertiseIdId(int id, string errorCode);
}
