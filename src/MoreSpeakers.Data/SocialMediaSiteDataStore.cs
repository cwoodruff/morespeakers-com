using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain;
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
    
    public async Task<Result<SocialMediaSite>> SaveAsync(SocialMediaSite socialMediaSite)
    {
        try
        {
            var dbSocialMediaSite = _mapper.Map<Models.SocialMediaSite>(socialMediaSite);
            if (socialMediaSite.Id != 0)
            {
                var tracked = _context.SocialMediaSite.Local.FirstOrDefault(e => e.Id == socialMediaSite.Id);
                if (tracked != null)
                    _context.Entry(tracked).State = EntityState.Detached;
            }
            _context.Entry(dbSocialMediaSite).State = socialMediaSite.Id == 0 ? EntityState.Added : EntityState.Modified;

            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToSaveSocialMediaSite(socialMediaSite.Id, socialMediaSite.Name);
                return Failure<SocialMediaSite>("social-media-site.save.failed", $"Failed to save social media site '{socialMediaSite.Name}'.");
            }

            return Result.Success(_mapper.Map<SocialMediaSite>(dbSocialMediaSite));
        }
        catch (DbUpdateException ex)
        {
            LogFailedToSaveSocialMediaSite(ex, socialMediaSite.Id, socialMediaSite.Name);
            return Failure<SocialMediaSite>("social-media-site.save.failed", $"Failed to save social media site '{socialMediaSite.Name}'.", ex);
        }
    }

    public async Task<Result<List<SocialMediaSite>>> GetAllAsync()
    {
        var socialMediaSites = await _context.SocialMediaSite.OrderBy(sms => sms.Name).ToListAsync();
        return Result.Success(_mapper.Map<List<SocialMediaSite>>(socialMediaSites));
    }
    
    public async Task<Result<SocialMediaSite>> GetAsync(int primaryKey)
    {
        var socialMediaSite = await _context.SocialMediaSite.FirstOrDefaultAsync(sms => sms.Id == primaryKey);
        if (socialMediaSite is null)
        {
            return Failure<SocialMediaSite>("social-media-site.not-found", $"Social media site {primaryKey} was not found.");
        }

        return Result.Success(_mapper.Map<SocialMediaSite>(socialMediaSite));
    }

    public Task<Result> DeleteAsync(SocialMediaSite entity) => DeleteAsync(entity.Id);

    public async Task<Result> DeleteAsync(int primaryKey)
    {
        try
        {
            var socialMediaSite = await _context.SocialMediaSite
                .FirstOrDefaultAsync(sms => sms.Id == primaryKey);
            
            if (socialMediaSite is null)
            {
                return Failure("social-media-site.delete.not-found", $"Social media site {primaryKey} was not found.");
            }
            
            _context.SocialMediaSite.Remove(socialMediaSite);

            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToDeleteSocialMediaSite(primaryKey);
                return Failure("social-media-site.delete.failed", $"Failed to delete social media site {primaryKey}.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToDeleteSocialMediaSite(ex, primaryKey);
            return Failure("social-media-site.delete.failed", $"Failed to delete social media site {primaryKey}.", ex);
        }
    }

    public async Task<Result<int>> RefCountAsync(int primaryKey)
    {
        var count = await _context.UserSocialMediaSite.CountAsync(x => x.SocialMediaSiteId == primaryKey);
        return Result.Success(count);
    }

    public async Task<Result<bool>> InUseAsync(int primaryKey)
    {
        var inUse = await _context.UserSocialMediaSite.AnyAsync(x => x.SocialMediaSiteId == primaryKey);
        return Result.Success(inUse);
    }

    private static Result Failure(string code, string message, Exception? exception = null) =>
        Result.Failure(new Error(code, message, exception));

    private static Result<T> Failure<T>(string code, string message, Exception? exception = null) =>
        Result.Failure<T>(new Error(code, message, exception));
}