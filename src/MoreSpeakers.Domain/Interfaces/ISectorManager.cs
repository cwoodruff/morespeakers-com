using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISectorManager
{
    Task<Result<Sector>> GetAsync(int id);
    Task<Result<Sector>> GetSectorWithRelationshipsAsync(int id);
    Task<Result<List<Sector>>> GetAllAsync();
    Task<Result<List<Sector>>> GetAllSectorsAsync(TriState active = TriState.True, string? searchTerm = "", bool includeCategories = false);
    Task<Result<Sector>> SaveAsync(Sector sector);
    Task<Result> DeleteAsync(int id);
    Task<Result> DeleteAsync(Sector sector);
}