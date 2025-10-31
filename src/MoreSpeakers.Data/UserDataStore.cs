using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Data;

public class UserDataStore : IUserDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly UserManager<Data.Models.User> _userManager;
    private readonly Mapper _mapper;

    public UserDataStore(MoreSpeakersDbContext context, UserManager<Data.Models.User> userManager)
    {
        _context = context;
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        });
        _mapper = new Mapper(mappingConfiguration);
        
        _userManager = userManager;
    }
    
    // ------------------------------------------
    // Wrapper methods for AspNetCore Identity
    // ------------------------------------------
    
    public async Task<User?> GetUserAsync(ClaimsPrincipal user)
    {
        var identityUser = await _userManager.GetUserAsync(user);
        
        return _mapper.Map<User>(identityUser);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var identityUser = _mapper.Map<Data.Models.User>(user);
        return await _userManager.ChangePasswordAsync(identityUser, currentPassword, newPassword);
    }

    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        var identityUser = _mapper.Map<Data.Models.User>(user);
        return await _userManager.CreateAsync(identityUser, password);
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

    public Task<IdentityResult> UpdateAsync(User user)
    {
        var identityUser = _mapper.Map<Data.Models.User>(user);
        return _userManager.UpdateAsync(identityUser);
    }

    // ------------------------------------------
    // Application Methods
    // ------------------------------------------
    
    public async Task<User> GetAsync(Guid primaryKey)
    {
        var speaker = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.SocialMediaLinks)
            .FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<User>(speaker);
    }

    public async Task<User> SaveAsync(User user)
    {
        var dbUser = _mapper.Map<Models.User>(user);
        _context.Entry(dbUser).State = EntityState.Detached;
        _context.Update(dbUser);
        var result = await _context.SaveChangesAsync() != 0;
        if (result)
        {
            return _mapper.Map<User>(dbUser);
        }

        throw new ApplicationException("Failed to save the user");
    }

    public async Task<List<User>> GetAllAsync()
    {
        var speakers = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
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
        return await _context.SaveChangesAsync() != 0;
    }

    public async Task<IEnumerable<User>> GetNewSpeakersAsync()
    {
        var users = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
            .Where(u => u.SpeakerType.Name == "NewSpeaker")
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
            .Include(u => u.SocialMediaLinks)
            .Where(u => u.SpeakerType.Name == "ExperiencedSpeaker")
            .OrderBy(u => u.FirstName)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<User>>(users);
    }

    public async Task<IEnumerable<User>> SearchSpeakersAsync(string searchTerm, int? speakerTypeId = null)
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
        
        return _mapper.Map<IEnumerable<User>>(users);
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

            var dbSocialMedia = _mapper.Map<Models.SocialMedia>(socialMedia);
            _context.SocialMedia.Add(dbSocialMedia);
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

            var dbUserExperience = _mapper.Map<Models.UserExpertise>(userExpertise);
            _context.UserExpertise.Add(dbUserExperience);
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

    public async Task<bool> EmptyAndAddExpertiseForUserAsync(Guid userId, int[] expertises)
    {
        try
        {
            var userExperiences = await _context.UserExpertise.ToListAsync();
            foreach (var userExpertise in userExperiences)
            {
                _context.UserExpertise.Remove(userExpertise);
            }

            foreach (var expertise in expertises)
            {
                _context.UserExpertise.Add(new Models.UserExpertise { UserId = userId, ExpertiseId = expertise });
            }
            return await _context.SaveChangesAsync() != 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EmptyAndAddSocialMediaForUserAsync(Guid userId, List<SocialMedia> socialMedias)
    {
        try
        {
            var socialMediaLinks = await _context.SocialMedia.ToListAsync();
            foreach (Models.SocialMedia socialMediaLink in socialMediaLinks)
            {
                _context.SocialMedia.Remove(socialMediaLink);
            }

            foreach (var socialMedia in socialMedias)
            {
                _context.SocialMedia.Add(new Models.SocialMedia { UserId = userId, Platform = socialMedia.Platform, Url = socialMedia.Url });           
            }

            return await _context.SaveChangesAsync() != 0;

        }
        catch
        {
            return false;
        }
    }

    public async Task<List<UserExpertise>> GetUserExpertisesForUserAsync(Guid userId)
    {
        var userExpertises = await _context.UserExpertise
            .Include(ue => ue.Expertise)
            .Where(ue => ue.UserId == userId)
            .ToListAsync();
        return _mapper.Map<List<UserExpertise>>(userExpertises);
    }

    public async Task<List<SocialMedia>> GetUserSocialMediaForUserAsync(Guid userId)
    {
        var socialMedias = await _context.SocialMedia
            .Where(sm => sm.UserId == userId)
            .ToListAsync();
        return _mapper.Map<List<SocialMedia>>(socialMedias);   
    }

    public async Task<(int newSpeakers, int experiencedSpeakers, int activeMentorships)> GetStatisticsForApplicationAsync()
    {
        var newSpeakers = await _context.Users.CountAsync(u => u.SpeakerType.Name == "NewSpeaker");

        var experiencedSpeakers = await _context.Users.CountAsync(u => u.SpeakerType.Name == "ExperiencedSpeaker");;

        var activeMentorships = await _context.Mentorship.CountAsync(m => m.Status == Models.MentorshipStatus.Active);
        
        return (newSpeakers, experiencedSpeakers, activeMentorships);

    }

    public async Task<List<User>> GetFeaturedSpeakersAsync(int count)
    {
        var speakers = await _context.Users
            .Where(s => !string.IsNullOrEmpty(s.Bio) && s.UserExpertise.Any())
            .OrderByDescending(s => s.UserExpertise.Count)
            .Take(count)
            .ToListAsync();
        
        return _mapper.Map<List<User>>(speakers);
    }
}