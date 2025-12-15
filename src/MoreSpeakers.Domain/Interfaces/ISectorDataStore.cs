using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISectorDataStore : IDataStorePrimaryKeyInt<Sector>
{
    Task<List<Sector>> GetAllAsync(bool onlyActive = true);
}