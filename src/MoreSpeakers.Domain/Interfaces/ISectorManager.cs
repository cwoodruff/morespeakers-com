using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISectorManager
{
    Task<Sector?> GetAsync(int id);
    Task<Sector?> GetSectorWithRelationshipsAsync(int id);
    Task<List<Sector>> GetAllAsync();
    Task<List<Sector>> GetAllSectorsAsync(TriState active = TriState.True, string? searchTerm = "", bool includeCategories = false);
    Task<Sector> SaveAsync(Sector sector);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAsync(Sector sector);
}