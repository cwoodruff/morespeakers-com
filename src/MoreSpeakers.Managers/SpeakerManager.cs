using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Managers;

public class SpeakerManager: ISpeakerManager
{
    private readonly ISpeakerDataStore _dataStore;
    private readonly ILogger<SpeakerManager> _logger;

    public SpeakerManager(ISpeakerDataStore dataStore, ILogger<SpeakerManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }
    
    
    public async Task<IEnumerable<Domain.Models.User>> GetNewSpeakersAsync()
    {
        var users = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
            .Where(u => u.SpeakerType.Name == "NewSpeaker")
            .OrderBy(u => u.FirstName)
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<Domain.Models.User>>(users);
    }

    public async Task<IEnumerable<Domain.Models.User>> GetExperiencedSpeakersAsync()
    {
        var users = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
            .Where(u => u.SpeakerType.Name == "ExperiencedSpeaker")
            .OrderBy(u => u.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<Domain.Models.User>>(users);
    }

    public async Task<Domain.Models.User?> GetSpeakerByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        return _mapper.Map<Domain.Models.User>(user);
    }

    public async Task<IEnumerable<Domain.Models.User>> SearchSpeakersAsync(string searchTerm, int? speakerTypeId = null)
    {
        var query = _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(u =>
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm) ||
                u.Bio.Contains(searchTerm) ||
                u.UserExpertise.Any(ue => ue.Expertise.Name.Contains(searchTerm)));

        if (speakerTypeId.HasValue) query = query.Where(u => u.SpeakerTypeId == speakerTypeId.Value);

        var users = await query.OrderBy(u => u.FirstName).ToListAsync();
        
        return _mapper.Map<IEnumerable<Domain.Models.User>>(users);
    }

    public async Task<IEnumerable<Domain.Models.User>> GetSpeakersByExpertiseAsync(int expertiseId)
    {
        var users = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId))
            .OrderBy(u => u.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<Domain.Models.User>>(users);
    }

    public async Task<bool> UpdateSpeakerProfileAsync(Domain.Models.User user)
    {
        var dbUser = _mapper.Map<User>(user);
        _context.Entry(dbUser).State =
            dbUser.Id == Guid.Empty ? EntityState.Added : EntityState.Modified;
        
        var result = await _context.SaveChangesAsync() != 0;
        return result ? true : throw new ApplicationException("Failed to save user profile");
    }

    public async Task<bool> AddSocialMediaLinkAsync(Guid userId, string platform, string url)
    {
        try
        {
            var socialMedia = new SocialMedia
            {
                UserId = userId,
                Platform = platform,
                Url = url
            };

            _context.SocialMedia.Add(socialMedia);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveSocialMediaLinkAsync(int socialMediaId)
    {
        try
        {
            var socialMedia = await _context.SocialMedia.FindAsync(socialMediaId);
            if (socialMedia != null)
            {
                _context.SocialMedia.Remove(socialMedia);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddExpertiseToUserAsync(Guid userId, int expertiseId)
    {
        try
        {
            var userExpertise = new UserExpertise
            {
                UserId = userId,
                ExpertiseId = expertiseId
            };

            _context.UserExpertise.Add(userExpertise);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId)
    {
        try
        {
            var userExpertise = await _context.UserExpertise
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpertiseId == expertiseId);

            if (userExpertise != null)
            {
                _context.UserExpertise.Remove(userExpertise);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}