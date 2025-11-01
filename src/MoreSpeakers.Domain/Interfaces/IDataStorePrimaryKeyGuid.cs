namespace MoreSpeakers.Domain.Interfaces;

public interface IDataStorePrimaryKeyGuid<T>: IDataStore<T> where T: class
{
    public Task<T> GetAsync(Guid primaryKey);
    public Task<bool> DeleteAsync(Guid primaryKey);
}