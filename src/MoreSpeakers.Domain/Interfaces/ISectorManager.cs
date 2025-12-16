using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISectorManager
{
    Task<Sector?> GetAsync(int id);
    Task<List<Sector>> GetAllAsync(bool onlyActive = true);
    Task<Sector> SaveAsync(Sector sector);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAsync(Sector sector);
}