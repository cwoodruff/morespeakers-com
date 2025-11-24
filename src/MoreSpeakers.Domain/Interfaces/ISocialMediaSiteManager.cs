using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface ISocialMediaSiteManager
{
    public Task<SocialMediaSite> GetAsync(int primaryKey);
    public Task<bool> DeleteAsync(int primaryKey);
    public Task<SocialMediaSite> SaveAsync(SocialMediaSite entity);
    public Task<List<SocialMediaSite>> GetAllAsync();
    public Task<bool> DeleteAsync(SocialMediaSite entity);
    
}