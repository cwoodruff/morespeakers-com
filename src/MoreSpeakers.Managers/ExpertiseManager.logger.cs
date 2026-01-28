using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Managers;

public partial class ExpertiseManager
{
    [LoggerMessage(LogLevel.Information, "Soft-deleted expertise with id {Id}")]
    partial void LogSoftDeletedExpertiseWithIdId(int id);

    [LoggerMessage(LogLevel.Warning, "Soft delete returned false for expertise id {Id}")]
    partial void LogSoftDeleteReturnedFalseForExpertiseIdId(int id);

    [LoggerMessage(LogLevel.Error, "Failed to create expertise '{Name}'")]
    partial void LogFailedToCreateExpertiseName(Exception ex, string name);

    [LoggerMessage(LogLevel.Error, "Failed to soft delete expertise id {Id}")]
    partial void LogFailedToSoftDeleteExpertiseIdId(Exception exception, int id);

    [LoggerMessage(LogLevel.Error, "Failed to soft delete expertise id {Id}")]
    partial void LogFailedToSoftDeleteExpertiseIdId(int id);
}