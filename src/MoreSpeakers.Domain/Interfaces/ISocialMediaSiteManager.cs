using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISocialMediaSiteManager
{
    Task<Result<SocialMediaSite>> GetAsync(int primaryKey);
    Task<Result<List<SocialMediaSite>>> GetAllAsync();
    Task<Result<SocialMediaSite>> SaveAsync(SocialMediaSite entity);
    Task<Result> DeleteAsync(int primaryKey);
    Task<Result> DeleteAsync(SocialMediaSite entity);
    Task<Result<int>> RefCountAsync(int primaryKey);
    Task<Result<bool>> InUseAsync(int primaryKey);
}