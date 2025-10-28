namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// Contains the database settings.
/// </summary>
public interface IDatabaseSettings
{
    /// <summary>
    /// The database connection string.
    /// </summary>
    public string DatabaseConnectionString { get; init; }
}