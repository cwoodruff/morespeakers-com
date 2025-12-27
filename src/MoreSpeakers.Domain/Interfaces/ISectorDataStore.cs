using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISectorDataStore : IDataStorePrimaryKeyInt<Sector>
{
    Task<Sector?> GetSectorWithRelationshipsAsync(int id);

    Task<List<Sector>> GetAllSectorsAsync(TriState active = TriState.True, string? searchTerm = "", bool includeCategories = false);
}