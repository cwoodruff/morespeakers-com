using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseDataStore
{
    Task<IEnumerable<Expertise>> GetAllExpertiseAsync();
    Task<Expertise?> GetExpertiseByIdAsync(int id);
    Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm);
    Task<bool> CreateExpertiseAsync(string name, string? description = null);
    Task<bool> UpdateExpertiseAsync(int id, string name, string? description = null);
    Task<bool> DeleteExpertiseAsync(int id);
    Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10);
}