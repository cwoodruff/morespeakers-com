using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseManager
{
    Task<bool> CreateExpertiseAsync(string name, string? description = null);
    Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10);
    Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm);
    Task<bool> UpdateExpertiseAsync(int id, string name, string? description = null);
}