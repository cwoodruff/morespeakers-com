namespace MoreSpeakers.Domain.Interfaces;

public interface IDataStore<T> where T: class
{
    public Task<T> SaveAsync(T entity);
    public Task<List<T>> GetAllAsync();
    public Task<bool> DeleteAsync(T entity);
}