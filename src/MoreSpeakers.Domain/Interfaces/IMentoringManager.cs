using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IMentoringManager
{
    public Task<Mentorship> GetAsync(Guid primaryKey);
    public Task<bool> DeleteAsync(Guid primaryKey);
    public Task<Mentorship> SaveAsync(Mentorship entity);
    public Task<List<Mentorship>> GetAllAsync();
    public Task<bool> DeleteAsync(Mentorship entity);
}