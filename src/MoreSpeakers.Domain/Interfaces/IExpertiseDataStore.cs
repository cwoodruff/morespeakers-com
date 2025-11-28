using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseDataStore: IDataStorePrimaryKeyInt<Expertise>
{
    Task<int> CreateExpertiseAsync(string name, string? description = null);
    Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10);
    Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm);
    Task<Expertise?> SearchForExpertiseExistsAsync(string name);
    Task<List<Expertise?>> FuzzySearchForExistingExpertise(string name, int count = 3);
}