using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseManager
{
    public Task<Expertise> GetAsync(int primaryKey);
    public Task<bool> DeleteAsync(int primaryKey);
    public Task<Expertise> SaveAsync(Expertise entity);
    public Task<List<Expertise>> GetAllAsync();
    public Task<bool> DeleteAsync(Expertise entity);
    
    Task<int> CreateExpertiseAsync(string name, string? description = null);
    Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10);
    Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm);
    Task<bool> UpdateExpertiseAsync(int id, string name, string? description = null);
    Task<Expertise?> SearchForExpertiseExistsAsync(string name);
    Task<List<Expertise>> FuzzySearchForExistingExpertise(string name, int count = 3);
}