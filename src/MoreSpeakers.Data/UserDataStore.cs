using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Data.Models;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

using SpeakerType = MoreSpeakers.Domain.Models.SpeakerType;
using User = MoreSpeakers.Domain.Models.User;
using UserExpertise = MoreSpeakers.Domain.Models.UserExpertise;

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
        var user = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.UserSocialMediaSites)
            .ThenInclude(sms => sms.SocialMediaSite)
            .FirstOrDefaultAsync(e => e.Email == email);
        return _mapper.Map<User?>(user);
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
        var user = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
            .FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<User?>(user);
    }

    public async Task<User> SaveAsync(User user)
    {
        return (user.Id == Guid.Empty) ? await AddUser(user) : await UpdateUser(user);
    }

    private async Task<User> AddUser(User user)
    {
        try
        {
            var dbUser = _mapper.Map<Models.User>(user);
            _context.Entry(dbUser).State = EntityState.Added;
                
            // Save the user expertise
            foreach (var expertise in user.UserExpertise)
            {
                var dbUserExpertise = _mapper.Map<Models.UserExpertise>(expertise);
                _context.UserExpertise.Add(dbUserExpertise);
            }
            
            // Save the user social media sites
            foreach (var socialMediaSite in user.UserSocialMediaSites)
            {
                var dbUserSocialMediaSite = _mapper.Map<Models.UserSocialMediaSites>(socialMediaSite);
                _context.UserSocialMediaSite.Add(dbUserSocialMediaSite);           
            }

            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<User>(dbUser);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add the new user");
        }
        throw new ApplicationException("Failed to add the new user");
    }

    private async Task<User> UpdateUser(User user)
    {
        try
        {
            var dbUser = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
                .FirstOrDefaultAsync(e => e.Id == user.Id);
            
            if (dbUser == null)
            {
                user.Id = Guid.NewGuid();
                return await AddUser(user);
            }
            
            // Custom mapping is needed to handle the UserExpertise and UserSocialMediaSites collections.
            // Entity Framework Core does not support updating collections.
            var map = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, Data.Models.User>()
                    .ForMember(d => d.UserExpertise, opt => opt.Ignore())
                    .ForMember(d => d.UserSocialMediaSites, opt => opt.Ignore());
                cfg.CreateMap<Models.MentorshipStatus, Domain.Models.MentorshipStatus>().ReverseMap();
                cfg.CreateMap<Models.MentorshipType, Domain.Models.MentorshipType>().ReverseMap();
                cfg.CreateMap<Models.SpeakerType, SpeakerType>().ReverseMap();
            });
            var mapper = map.CreateMapper();
            
            mapper.Map(user, dbUser);
            
            // Expertises
            var dbUserExpertisesIds = dbUser.UserExpertise.Select(ue => ue.ExpertiseId).ToList();
            var userExpertisesIds = user.UserExpertise.Select(ue => ue.ExpertiseId).ToList();
            
            // Expertises in user.UserExpertises but not in dbUser.UserExpertises (Add)
            var expertisesToAdd = userExpertisesIds.Except(dbUserExpertisesIds);
            foreach (var expertiseId in expertisesToAdd)
            {
                dbUser.UserExpertise.Add(new Data.Models.UserExpertise { UserId = dbUser.Id, ExpertiseId = expertiseId });
            }
            
            // Expertises in dbUser.UserExpertises but not in user.UserExpertises (Remove)
            var expertisesToRemove = dbUserExpertisesIds.Except(userExpertisesIds);
            foreach (var expertiseId in expertisesToRemove)
            {
                var dbUserExpertise = dbUser.UserExpertise.FirstOrDefault(ue => ue.ExpertiseId == expertiseId);
                if (dbUserExpertise != null)
                {
                    _context.UserExpertise.Remove(dbUserExpertise);
                }           
            }
            
            // Social Media Sites
            var dbUserSocialMediaSitesIds = dbUser.UserSocialMediaSites.Select(ue => ue.SocialMediaSiteId).ToList();
            var userSocialMediaSitesIds = user.UserSocialMediaSites.Select(ue => ue.SocialMediaSiteId).ToList();
            
            // Social Media Sites in user.UserSocialMediaSites but not in dbUser.UserSocialMediaSites (Add)
            var socialMediaSitesToAdd = userSocialMediaSitesIds.Except(dbUserSocialMediaSitesIds);
            foreach (var socialMediaSiteId in socialMediaSitesToAdd)
            {
                dbUser.UserSocialMediaSites.Add(new UserSocialMediaSites()
                {
                    UserId = dbUser.Id,
                    SocialMediaSiteId = socialMediaSiteId,
                    SocialId = user.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId)?.SocialId ?? string.Empty
                });
            }
            
            // Social Media Sites in dbUser.UserSocialMediaSites but not in user.UserSocialMediaSites (Remove)
            var socialMediaSitesToRemove = dbUserSocialMediaSitesIds.Except(userSocialMediaSitesIds);
            foreach (var socialMediaSiteId in socialMediaSitesToRemove)
            {
                var dbUserSocialMediaSite = dbUser.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId);
                if (dbUserSocialMediaSite != null)
                {
                    _context.UserSocialMediaSite.Remove(dbUserSocialMediaSite);
                }           
            }
            
            // Social Media sites that exist in both dbUser.UserSocialMediaSites and user.UserSocialMediaSites
            // Used to see what may need to be updated
            var socialMediaSitesToUpdate = dbUserSocialMediaSitesIds.Intersect(userSocialMediaSitesIds);
            foreach (var socialMediaSiteId in socialMediaSitesToUpdate)
            {
                var dbUserSocialMediaSite = dbUser.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId);
                if (dbUserSocialMediaSite != null)
                {
                    dbUserSocialMediaSite.SocialId = user.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId)?.SocialId ?? string.Empty;
                }           
            }
            
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<User>(dbUser);
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update the user '{Id}'", user.Id);
        }
        throw new ApplicationException($"Failed to update the user '{user.Id}'");
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
            .OrderBy(u => Guid.NewGuid())
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