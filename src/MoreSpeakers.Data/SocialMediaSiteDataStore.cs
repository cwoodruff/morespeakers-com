using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Data;

public class SocialMediaSiteDataStore: ISocialMediaSiteDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly Mapper _mapper;
    private readonly ILogger<SocialMediaSiteDataStore> _logger;

    public SocialMediaSiteDataStore(MoreSpeakersDbContext context, ILogger<SocialMediaSiteDataStore> logger)
    {
        _context = context;
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        });
        _mapper = new Mapper(mappingConfiguration);
        _logger = logger;
    }
    
    public async Task<SocialMediaSite> SaveAsync(SocialMediaSite socialMediaSite)
    { 
        var dbSocialMediaSite = _mapper.Map<Models.SocialMediaSite>(socialMediaSite);
        _context.Entry(dbSocialMediaSite).State = socialMediaSite.SocialMediaSiteId == 0 ? EntityState.Added : EntityState.Modified;

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<SocialMediaSite>(dbSocialMediaSite);
            }

            _logger.LogError("Failed to save the social media site. Id: '{Id}', Name: '{Name}'",
                socialMediaSite.SocialMediaSiteId, socialMediaSite.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Failed to save the social media site. Id: '{Id}', Name: '{Name}'",
                socialMediaSite.SocialMediaSiteId, socialMediaSite.Name);
        }
        throw new ApplicationException("Failed to save the social media site.");
    }

    public async Task<List<SocialMediaSite>> GetAllAsync()
    {
        var socialMediaSites = await _context.SocialMediaSite.OrderBy(sms => sms.Name).ToListAsync();
        return _mapper.Map<List<SocialMediaSite>>(socialMediaSites);
    }
    
    public async Task<SocialMediaSite> GetAsync(int primaryKey)
    {
        var socialMediaSite = await _context.SocialMediaSite.FirstOrDefaultAsync(sms => sms.SocialMediaSiteId == primaryKey);
        return _mapper.Map<SocialMediaSite>(socialMediaSite);
    }

    public async Task<bool> DeleteAsync(SocialMediaSite entity)
    {
        return await DeleteAsync(entity.SocialMediaSiteId);
    }

    public async Task<bool> DeleteAsync(int primaryKey)
    {
        var socialMediaSite = await _context.SocialMediaSite
            .FirstOrDefaultAsync(sms => sms.SocialMediaSiteId == primaryKey);
        
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
            _logger.LogError("Failed to delete social media site with id: '{Id}'", primaryKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete social media site with id: '{Id}'", primaryKey);
        }
        return false;
    }
}