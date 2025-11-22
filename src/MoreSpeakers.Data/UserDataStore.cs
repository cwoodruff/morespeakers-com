using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Data;

public class UserDataStore : IUserDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly UserManager<Data.Models.User> _userManager;
    private readonly Mapper _mapper;
    private readonly ILogger<UserDataStore> _logger;

    public UserDataStore(MoreSpeakersDbContext context, UserManager<Data.Models.User> userManager, ILogger<UserDataStore> logger)
    {
        _context = context;
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        });
        _mapper = new Mapper(mappingConfiguration);
        
        _userManager = userManager;
        _logger = logger;
    }
    
    // ------------------------------------------
    // Wrapper methods for AspNetCore Identity
    // ------------------------------------------
    
    public async Task<User?> GetUserAsync(ClaimsPrincipal user)
    {
        var identityUserByClaim = await _userManager.GetUserAsync(user);
        return _mapper.Map<User>(identityUserByClaim);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        try
        {
            // Need to load the user from the "Identity" manager to change the password
            var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (identityUser == null)
            {
                _logger.LogError("Could not find user with id: {UserId} in the Identity database", user.Id);
                throw new InvalidOperationException(
                    $"Could not find user with id: {user.Id} in the Identity database.");
            }

            var result = await _userManager.ChangePasswordAsync(identityUser, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to change password for user with id: {UserId}", user.Id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change password for user with id: {UserId}", user.Id);
            return IdentityResult.Failed();
        }
    }

    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        var identityUser = _mapper.Map<Data.Models.User>(user);

        try
        {
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user with id: {UserId}", user.Id);                
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with id: {UserId}", user.Id);
            return IdentityResult.Failed();
        }
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        var identityUser = await _userManager.FindByEmailAsync(email);
        return _mapper.Map<User>(identityUser);
    }

    public async Task<User?> GetUserIdAsync(ClaimsPrincipal user)
    {
        var identityUser = await _userManager.GetUserAsync(user);
        return _mapper.Map<User>(identityUser);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        var identityUser = _mapper.Map<Data.Models.User>(user);
        return await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());

        if (identityUser == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });       
        }
        
        return await _userManager.ConfirmEmailAsync(identityUser, token);
    }


    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    public async Task<User?> GetAsync(Guid primaryKey)
    {
        var speaker = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
            .FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<User?>(speaker);
    }

    public async Task<User> SaveAsync(User user)
    {
        try
        {
            Data.Models.User? dbUser;
            if (user.Id != Guid.Empty)
            {
                dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (dbUser == null)
                {
                    dbUser = _mapper.Map<Models.User>(user);
                    _context.Entry(dbUser).State = EntityState.Added;
                }
                else
                {
                    _context.ChangeTracker.Clear();
                }
                // Let's remove all the fk references to the user before saving
                // This is to avoid the "circular dependency" error when saving the user
                var userExpertise = await _context.UserExpertise.Where(ue => ue.UserId == user.Id).ToListAsync();
                foreach (var userExpertiseItem in userExpertise)
                {
                    _context.UserExpertise.Remove(userExpertiseItem);
                }
                var userSocialMediaSites = await _context.UserSocialMediaSite.Where(sms => sms.UserId == user.Id).ToListAsync();
                foreach (var userSocialMediaSiteItem in userSocialMediaSites)
                {
                    _context.UserSocialMediaSite.Remove(userSocialMediaSiteItem);
                }
                await _context.SaveChangesAsync();
                dbUser = _mapper.Map<Models.User>(user);
                _context.Entry(dbUser).State = EntityState.Modified;
            }
            else
            {
                dbUser = _mapper.Map<Models.User>(user);
                _context.Entry(dbUser).State = EntityState.Added;
            }

            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<User>(dbUser);
            }

            _logger.LogError("Failed to save the user. Id: '{Id}'", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the user. Id: '{Id}'", user.Id);
        }

        throw new ApplicationException("Failed to save the user");
    }

    public async Task<List<User>> GetAllAsync()
    {
        var speakers = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.UserSocialMediaSites)
            .ThenInclude(sms => sms.SocialMediaSite)
            .ToListAsync();
        return _mapper.Map<List<User>>(speakers);
    }

    public async Task<bool> DeleteAsync(User entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public async Task<bool> DeleteAsync(Guid primaryKey)
    {
        var speaker = await _context.Users
            .Include(u => u.UserExpertise)
            .FirstOrDefaultAsync(e => e.Id == primaryKey);

        if (speaker is null)
        {
            return true;
        }

        foreach (var userExpertise in speaker.UserExpertise)
        {
            _context.UserExpertise.Remove(userExpertise);
        }
        _context.Users.Remove(speaker);

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return true;
            }
            _logger.LogError("Failed to delete the user. Id: '{Id}'", primaryKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the user. Id: '{Id}'", primaryKey);
        }
        return false;
    }

    public async Task<IEnumerable<User>> GetNewSpeakersAsync()
    {
        var users = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.UserSocialMediaSites)
            .ThenInclude(sms => sms.SocialMediaSite)
            .Where(u => u.SpeakerType.Id == (int)SpeakerTypeEnum.NewSpeaker)
            .OrderBy(u => u.FirstName)
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<User>>(users);
    }

    public async Task<IEnumerable<User>> GetExperiencedSpeakersAsync()
    {
        var users = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.UserSocialMediaSites)
            .ThenInclude(sms => sms.SocialMediaSite)
            .Where(u => u.SpeakerType.Id == (int)SpeakerTypeEnum.ExperiencedSpeaker)
            .OrderBy(u => u.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<User>>(users);
    }

    public async Task<SpeakerSearchResult> SearchSpeakersAsync(string? searchTerm, int? speakerTypeId = null, int? expertiseId = null, SpeakerSearchOrderBy sortOrder = SpeakerSearchOrderBy.Name, int? page = null, int? pageSize = null)
    {

        var query = _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .AsQueryable();

        // Search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm) ||
                u.Bio.Contains(searchTerm) ||
                u.UserExpertise.Any(ue => ue.Expertise.Name.Contains(searchTerm)));
        }

        // Speaker type
        if (speakerTypeId.HasValue)
        {
            query = query.Where(u => u.SpeakerTypeId == speakerTypeId.Value);
        }
        
        // Expertise
        if (expertiseId.HasValue)
        {
            query = query.Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId.Value));
        }

        // Sort Order
        switch (sortOrder)
        {
            case SpeakerSearchOrderBy.Newest:
                query = query.OrderByDescending(u => u.CreatedDate);
                break;
            
            case SpeakerSearchOrderBy.Expertise:
                query = query.OrderBy(u => u.UserExpertise.Count).ThenBy(u => u.LastName).ThenBy(u => u.FirstName);
                break;
            
            case SpeakerSearchOrderBy.Name:
            default:
                query = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);
                break;
        }

        var users = await query.ToListAsync();
        var totalCount = users.Count;
        
        if (page.HasValue && pageSize.HasValue)
        {
            users = users.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
        }

        if (page.HasValue)
        {
            
        }
        var results = new SpeakerSearchResult
        {
            RowCount = totalCount,
            Speakers = _mapper.Map<IEnumerable<User>>(users),
            PageSize = pageSize ?? totalCount,
            CurrentPage = page ?? 1,
            TotalPages = page.HasValue ? (totalCount / (pageSize ?? totalCount)) : 1
        };
        
        return results;
    }

    public async Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId)
    {
        var users = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId))
            .OrderBy(u => u.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<User>>(users);
    }

    public async Task<bool> AddUserSocialMediaSiteAsync(Guid userId, UserSocialMediaSite userSocialMediaSite)
    {
        try
        {
            var dbUserSocialMediaSite = _mapper.Map<Models.UserSocialMediaSites>(userSocialMediaSite);
            _context.UserSocialMediaSite.Add(dbUserSocialMediaSite);

            var result = await _context.SaveChangesAsync() != 0;

            if (!result)
            {
                _logger.LogError(
                    "Failed to add social media link for user for id: {UserId}. SocialMediaSiteId: {SocialMediaSiteId}, SocialId: {SocialId}",
                    userId, userSocialMediaSite.SocialMediaSiteId, userSocialMediaSite.SocialId);
            }
            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to add social media link for user for id: {UserId}. SocialMediaSiteId: {SocialMediaSiteId}, SocialId: {SocialId}",
                userId, userSocialMediaSite.SocialMediaSiteId, userSocialMediaSite.SocialId);
            return false;
        }
    }

    public async Task<bool> RemoveUserSocialMediaSiteAsync(int userSocialMediaSiteId)
    {
        try
        {
            var userSocialMediaSite = await _context.UserSocialMediaSite.FindAsync(userSocialMediaSiteId);
            if (userSocialMediaSite == null)
            {
                return false;
            }

            _context.UserSocialMediaSite.Remove(userSocialMediaSite);
            var result = await _context.SaveChangesAsync() != 0;
            if (!result)
            {
                _logger.LogError("Failed to remove social media link with id: {UserSocialMediaSiteId}", userSocialMediaSiteId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove social media link with id: {UserSocialMediaSiteId}", userSocialMediaSiteId);
            return false;
        }
    }
    
    public async Task<bool> EmptyAndAddUserSocialMediaSiteForUserAsync(Guid userId, Dictionary<int, string> userSocialMediaSites)
    {
        try
        {
            var userSocialMediaSitesList = await _context.UserSocialMediaSite.Where(u => u.UserId == userId).ToListAsync();
            foreach (Models.UserSocialMediaSites userSocialMediaSite in userSocialMediaSitesList)
            {
                _context.UserSocialMediaSite.Remove(userSocialMediaSite);
            }

            foreach (var userSocialMediaSite in userSocialMediaSites)
            {
                _context.UserSocialMediaSite.Add(new Models.UserSocialMediaSites
                {
                    SocialId = userSocialMediaSite.Value,
                    SocialMediaSiteId = userSocialMediaSite.Key,
                    UserId = userId
                });
            }

            var result = await _context.SaveChangesAsync() != 0;
            if (!result)
            {
                _logger.LogError("Failed to empty and add social media links to user with id: {UserId}", userId);
            }
            return result;       

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to empty and add social media links to user with id: {UserId}", userId);
            return false;
        }
    }
    
    public async Task<IEnumerable<UserSocialMediaSite>> GetUserSocialMediaSitesAsync(Guid userId)
    {
        var userSocialMediaSitesList = await _context.UserSocialMediaSite
            .Where(sm => sm.UserId == userId)
            .ToListAsync();
        return _mapper.Map<List<UserSocialMediaSite>>(userSocialMediaSitesList);   
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

            var dbUserExperience = _mapper.Map<Models.UserExpertise>(userExpertise);
            _context.UserExpertise.Add(dbUserExperience);
            var result = await _context.SaveChangesAsync() != 0;
            if (!result)
            {
                _logger.LogError("Failed to add expertise to user with id: {UserId}. ExpertiseId: {ExpertiseId}", userId, expertiseId);
            }
            return result;
        }
        catch( Exception ex)
        {
            _logger.LogError(ex, "Failed to add expertise to user with id: {UserId}. ExpertiseId: {ExpertiseId}", userId, expertiseId);
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
                var result = await _context.SaveChangesAsync() != 0;
                if (!result)
                {
                    _logger.LogError("Failed to remove expertise from user with id: {UserId}. ExpertiseId: {ExpertiseId}", userId, expertiseId);
                }
                return result;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove expertise from user with id: {UserId}. ExpertiseId: {ExpertiseId}", userId, expertiseId);
            return false;
        }
    }

    public async Task<bool> EmptyAndAddExpertiseForUserAsync(Guid userId, int[] expertises)
    {
        try
        {
            var userExperiences = await _context.UserExpertise.Where(u => u.UserId == userId).ToListAsync();
            foreach (var userExpertise in userExperiences)
            {
                _context.UserExpertise.Remove(userExpertise);
            }

            foreach (var expertise in expertises)
            {
                _context.UserExpertise.Add(new Models.UserExpertise { UserId = userId, ExpertiseId = expertise });
            }
            var result = await _context.SaveChangesAsync() != 0;
            if (!result)
            {
                _logger.LogError("Failed to empty and add expertise to user with id: {UserId}", userId);
            }
            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to empty and add expertise to user with id: {UserId}", userId);
            return false;
        }
    }
    
    public async Task<IEnumerable<UserExpertise>> GetUserExpertisesForUserAsync(Guid userId)
    {
        var userExpertises = await _context.UserExpertise
            .Include(ue => ue.Expertise)
            .Where(ue => ue.UserId == userId)
            .ToListAsync();
        return _mapper.Map<List<UserExpertise>>(userExpertises);
    }


    public async Task<(int newSpeakers, int experiencedSpeakers, int activeMentorships)> GetStatisticsForApplicationAsync()
    {
        var newSpeakers = await _context.Users.CountAsync(u => u.SpeakerType.Id == (int) SpeakerTypeEnum.NewSpeaker);

        var experiencedSpeakers = await _context.Users.CountAsync(u => u.SpeakerType.Id == (int) SpeakerTypeEnum.ExperiencedSpeaker);

        var activeMentorships = await _context.Mentorship.CountAsync(m => m.Status == Models.MentorshipStatus.Active);
        
        return (newSpeakers, experiencedSpeakers, activeMentorships);

    }

    public async Task<IEnumerable<User>> GetFeaturedSpeakersAsync(int count)
    {
        var speakers = await _context.Users
            .Where(s => !string.IsNullOrEmpty(s.Bio) && s.UserExpertise.Any() && s.SpeakerType.Id == (int) SpeakerTypeEnum.ExperiencedSpeaker)
            .OrderByDescending(s => s.UserExpertise.Count)
            .Take(count)
            .ToListAsync();
        
        return _mapper.Map<List<User>>(speakers);
    }

    public async Task<IEnumerable<SpeakerType>> GetSpeakerTypesAsync()
    {
        var speakerTypes = await _context.SpeakerType.ToListAsync();
        return _mapper.Map<List<SpeakerType>>(speakerTypes);
    }
}