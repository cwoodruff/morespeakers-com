using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Data;

public partial class SectorDataStore
{
    [LoggerMessage(LogLevel.Error, "Failed to save the sector. Name: '{Name}'")]
    partial void LogFailedToSaveSector(string name);

    [LoggerMessage(LogLevel.Error, "Failed to save the sector. Name: '{Name}'")]
    partial void LogFailedToSaveSector(Exception exception, string name);

    [LoggerMessage(LogLevel.Error, "Failed to delete the sector. Name: '{Name}'")]
    partial void LogFailedToDeleteSector(Exception exception, string name);
}