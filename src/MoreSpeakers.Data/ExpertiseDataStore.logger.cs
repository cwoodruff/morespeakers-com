using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Data;

public partial class ExpertiseDataStore
{
    [LoggerMessage(LogLevel.Error, "Failed to delete the expertise. Name: '{Name}'")]
    partial void LogFailedToDeleteExpertise(Exception exception, string name);

    [LoggerMessage(LogLevel.Error, "Failed to save the expertise. Name: '{Name}'")]
    partial void LogFailedToSaveExpertise(string name);

    [LoggerMessage(LogLevel.Error, "Failed to save the expertise. Name: '{Name}'")]
    partial void LogFailedToSaveTheExpertiseNameName(Exception exception, string name);

    [LoggerMessage(LogLevel.Error, "Failed to create the expertise. Name: '{Name}'")]
    partial void LogFailedToCreateExpertise(string name);

    [LoggerMessage(LogLevel.Error, "Failed to create the expertise. Name: '{Name}'")]
    partial void LogFailedToCreateExpertise(Exception exception, string name);

    [LoggerMessage(LogLevel.Error, "Failed to soft delete expertise id {Id}")]
    partial void LogFailedToSoftDeleteExpertise(Exception exception, int id);

    [LoggerMessage(LogLevel.Error, "Failed to save the expertise category. Name: '{Name}'")]
    partial void LogFailedToSaveExpertiseCategory(string name);

    [LoggerMessage(LogLevel.Error, "Failed to save the expertise category. Name: '{Name}'")]
    partial void LogFailedToSaveExpertiseCategory(Exception exception, string name);

    [LoggerMessage(LogLevel.Warning, "Attempted to delete category with id {Id} that still has expertises")]
    partial void LogAttemptedToDeleteCategoryWithExpertises(int id);

    [LoggerMessage(LogLevel.Error, "Failed to delete the expertise category. Name: '{Name}'")]
    partial void LogFailedToDeleteExpertiseCategory(Exception ex, string name);
}