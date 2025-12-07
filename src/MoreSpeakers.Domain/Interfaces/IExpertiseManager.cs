using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseManager
{
    public Task<Expertise> GetAsync(int primaryKey);
    public Task<bool> DeleteAsync(int primaryKey);
    public Task<bool> DeleteAsync(Expertise entity);
    public Task<Expertise> SaveAsync(Expertise entity);
    public Task<List<Expertise>> GetAllAsync();
    Task<int> CreateExpertiseAsync(string name, string? description = null);
    Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10);
    Task<bool> DoesExpertiseWithNameExistsAsync(string expertiseName);
    Task<IEnumerable<Expertise>> FuzzySearchForExistingExpertise(string name, int count = 3);
}