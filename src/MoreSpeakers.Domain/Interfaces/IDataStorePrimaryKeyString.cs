namespace MoreSpeakers.Domain.Interfaces;

public interface IDataStorePrimaryKeyString<T>: IDataStore<T> where T: class
{
    public Task<T?> GetAsync(string primaryKey);
    public Task<bool> DeleteAsync(string primaryKey);
}