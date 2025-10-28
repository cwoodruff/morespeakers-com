using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Domain.Models;

/// <summary>
/// The database settings.
/// </summary>
public class DatabaseSettings: IDatabaseSettings
{
    public string DatabaseConnectionString { get; init; }
}