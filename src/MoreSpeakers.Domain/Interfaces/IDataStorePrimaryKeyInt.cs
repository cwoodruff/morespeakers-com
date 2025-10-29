namespace MoreSpeakers.Domain.Interfaces;

public interface IDataStorePrimaryKeyInt<T>: IDataStore<T> where T: class
{
    public Task<T> GetAsync(int primaryKey);
    public Task<bool> DeleteAsync(int primaryKey);
}