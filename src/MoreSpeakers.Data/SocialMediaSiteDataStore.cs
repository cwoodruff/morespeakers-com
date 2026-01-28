using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Data;

public partial class SocialMediaSiteDataStore: ISocialMediaSiteDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SocialMediaSiteDataStore> _logger;

    public SocialMediaSiteDataStore(MoreSpeakersDbContext context, IMapper mapper, ILogger<SocialMediaSiteDataStore> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<SocialMediaSite> SaveAsync(SocialMediaSite socialMediaSite)
    { 
        var dbSocialMediaSite = _mapper.Map<Models.SocialMediaSite>(socialMediaSite);
        _context.Entry(dbSocialMediaSite).State = socialMediaSite.Id == 0 ? EntityState.Added : EntityState.Modified;

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<SocialMediaSite>(dbSocialMediaSite);
            }

            LogFailedToSaveSocialMediaSite(socialMediaSite.Id, socialMediaSite.Name);
        }
        catch (Exception ex)
        {
            LogFailedToSaveSocialMediaSite(ex, socialMediaSite.Id, socialMediaSite.Name);
        }
        throw new ApplicationException("Failed to save the social media site.");
    }

    public async Task<List<SocialMediaSite>> GetAllAsync()
    {
        var socialMediaSites = await _context.SocialMediaSite.OrderBy(sms => sms.Name).ToListAsync();
        return _mapper.Map<List<SocialMediaSite>>(socialMediaSites);
    }
    
    public async Task<SocialMediaSite?> GetAsync(int primaryKey)
    {
        var socialMediaSite = await _context.SocialMediaSite.FirstOrDefaultAsync(sms => sms.Id == primaryKey);
        return _mapper.Map<SocialMediaSite>(socialMediaSite);
    }

    public async Task<bool> DeleteAsync(SocialMediaSite entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public async Task<bool> DeleteAsync(int primaryKey)
    {
        var socialMediaSite = await _context.SocialMediaSite
            .FirstOrDefaultAsync(sms => sms.Id == primaryKey);
        
        if (socialMediaSite is null)
        {
            return true;
        }
        
        _context.SocialMediaSite.Remove(socialMediaSite);

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return true;
            }
            LogFailedToDeleteSocialMediaSite(primaryKey);
        }
        catch (Exception ex)
        {
            LogFailedToDeleteSocialMediaSite(ex, primaryKey);
        }
        return false;
    }

    public async Task<int> RefCountAsync(int primaryKey)
    {
        return await _context.UserSocialMediaSite.CountAsync(x => x.SocialMediaSiteId == primaryKey);
    }

    public async Task<bool> InUseAsync(int primaryKey)
    {
        return await _context.UserSocialMediaSite.AnyAsync(x => x.SocialMediaSiteId == primaryKey);
    }
}