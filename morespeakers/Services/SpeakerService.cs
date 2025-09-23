using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;

namespace morespeakers.Services;

// Speaker Service Implementation
public class SpeakerService(ApplicationDbContext context) : ISpeakerService
{
    public async Task<IEnumerable<User>> GetNewSpeakersAsync()
    {
        return await context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
            .Where(u => u.SpeakerType.Name == "NewSpeaker")
            .OrderBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetExperiencedSpeakersAsync()
    {
        return await context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
            .Where(u => u.SpeakerType.Name == "ExperiencedSpeaker")
            .OrderBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<User?> GetSpeakerByIdAsync(Guid id)
    {
        return await context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.SocialMediaLinks)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> SearchSpeakersAsync(string searchTerm, int? speakerTypeId = null)
    {
        var query = context.Users
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

        return await query.OrderBy(u => u.FirstName).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetSpeakersByExpertiseAsync(int expertiseId)
    {
        return await context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId))
            .OrderBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<bool> UpdateSpeakerProfileAsync(User user)
    {
        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
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

            context.SocialMedia.Add(socialMedia);
            await context.SaveChangesAsync();
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
            var socialMedia = await context.SocialMedia.FindAsync(socialMediaId);
            if (socialMedia != null)
            {
                context.SocialMedia.Remove(socialMedia);
                await context.SaveChangesAsync();
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

            context.UserExpertise.Add(userExpertise);
            await context.SaveChangesAsync();
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
            var userExpertise = await context.UserExpertise
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpertiseId == expertiseId);

            if (userExpertise != null)
            {
                context.UserExpertise.Remove(userExpertise);
                await context.SaveChangesAsync();
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