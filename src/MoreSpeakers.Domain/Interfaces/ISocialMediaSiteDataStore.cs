using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISocialMediaSiteDataStore: IDataStorePrimaryKeyInt<SocialMediaSite>
{
    Task<int> RefCountAsync(int primaryKey);
    Task<bool> InUseAsync(int primaryKey);
}