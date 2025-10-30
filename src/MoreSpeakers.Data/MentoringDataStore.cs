using AutoMapper;

using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

using Mentorship = MoreSpeakers.Domain.Models.Mentorship;

namespace MoreSpeakers.Data;

public class MentoringDataStore: IMentoringDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly Mapper _mapper;

    public MentoringDataStore(IDatabaseSettings databaseSettings)
    {
        _context = new MoreSpeakersDbContext(databaseSettings);
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        });
        _mapper = new Mapper(mappingConfiguration);
    }

    public async Task<Mentorship> GetAsync(Guid primaryKey)
    {
        var mentorship = await _context.Mentorship.FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<Mentorship>(mentorship);
    }

    public async Task<Mentorship> SaveAsync(Mentorship entity)
    {
        var expertise = _mapper.Map<Models.Mentorship>(entity);
        _context.Entry(entity).State = entity.Id == Guid.Empty ? EntityState.Added : EntityState.Modified;

        var result = await _context.SaveChangesAsync() != 0;
        if (result)
        {
            return _mapper.Map<Mentorship>(expertise);
        }

        throw new ApplicationException("Failed to save the expertise");
    }

    public async Task<List<Mentorship>> GetAllAsync()
    {
        var mentorships = await _context.Mentorship.ToListAsync();
        return _mapper.Map<List<Mentorship>>(mentorships);
    }

    public async Task<bool> DeleteAsync(Mentorship entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public async Task<bool> DeleteAsync(Guid primaryKey)
    {
        var mentorship = await _context.Mentorship
            .FirstOrDefaultAsync(e => e.Id == primaryKey);

        if (mentorship is null)
        {
            return true;
        }

        _context.Mentorship.Remove(mentorship);
        return await _context.SaveChangesAsync() != 0;
    }

    public async Task<List<Expertise>> GetSharedExpertisesAsync(User mentor, User mentee)
    {
        var mentorExpertises = await _context.UserExpertise
            .Where(ue => ue.UserId == mentor.Id)
            .Select(ue => ue.ExpertiseId)
            .ToListAsync();
        var menteeExpertises = await _context.UserExpertise
            .Where(ue => ue.UserId == mentee.Id)
            .Select(ue => ue.ExpertiseId)
            .ToListAsync();
        
        var expertises = await _context.Expertise
            .Where(e => mentorExpertises.Contains(e.Id) && menteeExpertises.Contains(e.Id))
            .ToListAsync();
        
        return _mapper.Map<List<Expertise>>(expertises);
    }

    public async Task<bool> DoesMentorshipRequestsExistsAsync(User mentor, User mentee)
    {
        var exists = await _context.Mentorship.FirstOrDefaultAsync(m => m.MentorId == mentor.Id && m.MenteeId == mentee.Id && m.Status == Models.MentorshipStatus.Pending);
        
        return exists != null;
    }

    public async Task<bool> CreateMentorshipRequestAsync(Mentorship mentorship, List<int> expertiseIds)
    {
        var dbMentorship = _mapper.Map<Data.Models.Mentorship>(mentorship);
        
        _context.Mentorship.Add(dbMentorship);
        var created = await _context.SaveChangesAsync();
        if (created == 0)
        {
            return false;
        }
        
        foreach (var expertiseId in expertiseIds)
        {
            await _context.UserExpertise.AddAsync(new Data.Models.UserExpertise
            {
                UserId = mentorship.MentorId,
                ExpertiseId = expertiseId
            });
        }
        
        return await _context.SaveChangesAsync() != 0;
        
    }

    public async Task<Mentorship?> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null)
    {
        var mentorship = await _context.Mentorship
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && m.MentorId == userId);

        if (mentorship is null)
        {
            return null;
        }
        
        mentorship.Status = accepted ? Models.MentorshipStatus.Active : Models.MentorshipStatus.Declined;
        mentorship.ResponseMessage = message;
        mentorship.ResponsedAt = DateTime.UtcNow;
        mentorship.UpdatedAt = DateTime.UtcNow;

        if (accepted)
        {
            mentorship.StartedAt = DateTime.UtcNow;
        }

        var saved = await _context.SaveChangesAsync();
        if (saved == 0)
        {
            return null;
        }
        
        return _mapper.Map<Mentorship>(mentorship);
    }

    public async Task<List<Mentorship>> GetActiveMentorshipsForUserAsync(Guid userId)
    {
        var activeMentorships = await _context.Mentorship
            .Include(m => m.Mentor)
            .ThenInclude(m => m.SpeakerType)
            .Include(m => m.Mentee)
            .ThenInclude(m => m.SpeakerType)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => (m.MentorId == userId || m.MenteeId == userId) && 
                        m.Status == Models.MentorshipStatus.Active)
            .OrderBy(m => m.StartedAt)
            .ToListAsync();
        
        return _mapper.Map<List<Mentorship>>(activeMentorships);
    }

    public async Task<(int outboundCount, int inboundCount)> GetNumberOfMentorshipsPending(Guid userId)
    {
        var outboundCount = await _context.Mentorship.CountAsync(m => m.MenteeId == userId && m.Status == Models.MentorshipStatus.Pending);
        var inboundCount = await _context.Mentorship.CountAsync(m => m.MentorId == userId && m.Status == Models.MentorshipStatus.Pending);
        
        return (outboundCount, inboundCount);
    }

    public async Task<List<Mentorship>> GetIncomingMentorshipRequests(Guid userId)
    {
        var mentorships = await _context.Mentorship
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MentorId == userId && m.Status == Models.MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
        return _mapper.Map<List<Mentorship>>(mentorships);
    }

    public async Task<List<Mentorship>> GetOutgoingMentorshipRequests(Guid userId)
    {
        var mentorships = await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MenteeId == userId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
        return _mapper.Map<List<Mentorship>>(mentorships); 
    }

    public async Task<bool> CancelMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var mentorship = await _context.Mentorship
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && (m.MenteeId == userId || m.MentorId == userId));

        if (mentorship is null)
        {
            return false;
        }
        mentorship.Status = Models.MentorshipStatus.Cancelled;
        mentorship.UpdatedAt = DateTime.UtcNow;
        _context.Mentorship.Remove(mentorship);
        return await _context.SaveChangesAsync() != 0;
    }
    
    public async Task<bool> CompleteMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var mentorship = await _context.Mentorship
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && (m.MenteeId == userId || m.MentorId == userId));

        if (mentorship is null)
        {
            return false;
        }
        mentorship.Status = Models.MentorshipStatus.Completed;
        mentorship.UpdatedAt = DateTime.UtcNow;
        _context.Mentorship.Remove(mentorship);
        return await _context.SaveChangesAsync() != 0;
    }

    public async Task<List<User>> GetMentorsExceptForUserAsync(Guid userId, MentorshipType mentorshipType, List<string>? expertiseNames, bool? availability = true)
    {
        var query = _context.Users
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.Id != userId);

        if (expertiseNames is not null && expertiseNames.Count > 0)
        {
            var expertiseIds = await _context.Expertise
                .Where(e => expertiseNames.Contains(e.Name))
                .Select(e => e.Id)
                .ToListAsync();

            query = query.Where(u => u.UserExpertise.Any(ue => expertiseIds.Contains(ue.ExpertiseId)));
        }

        query = query.Where(u => u.IsAvailableForMentoring == availability);
        
        var mentors = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
        
        return _mapper.Map<List<User>>(mentors);
    }

    public async Task<User?> GetMentorAsync(Guid userId)
    {
        var mentor = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        return _mapper.Map<User?>(mentor);
    }
    
    public async Task<bool> CanRequestMentorshipAsync(Guid menteeId, Guid mentorId)
    {
        // Check if there's already a pending or active mentorship
        var existingMentorship = await _context.Mentorship
            .AnyAsync(m =>
                ((m.MenteeId == menteeId && m.MentorId == mentorId) ||
                 (m.MenteeId == mentorId && m.MentorId == menteeId)) &&
                (m.Status == Models.MentorshipStatus.Pending || m.Status == Models.MentorshipStatus.Active));

        return !existingMentorship;
    }
    
    public async Task<Mentorship?> RequestMentorshipWithDetailsAsync(Guid requesterId, Guid targetId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency)
    {
        try
        {
            // Check if there's already a pending or active mentorship between these users
            var existingMentorship = await _context.Mentorship
                .FirstOrDefaultAsync(m =>
                    ((m.MenteeId == requesterId && m.MentorId == targetId) ||
                     (m.MenteeId == targetId && m.MentorId == requesterId)) &&
                    (m.Status == Models.MentorshipStatus.Pending || m.Status == Models.MentorshipStatus.Active));

            if (existingMentorship != null)
                return null;

            var dbMentorshipType = _mapper.Map<Data.Models.MentorshipType>(type);
            
            var mentorship = new Models.Mentorship
            {
                Id = Guid.NewGuid(),
                MentorId = targetId,
                MenteeId = requesterId,
                Status = Models.MentorshipStatus.Pending,
                Type = dbMentorshipType,
                RequestedAt = DateTime.UtcNow,
                RequestMessage = requestMessage,
                PreferredFrequency = preferredFrequency
            };

            _context.Mentorship.Add(mentorship);
            await _context.SaveChangesAsync();

            // Add focus areas if provided
            if (focusAreaIds != null && focusAreaIds.Any())
            {
                foreach (var expertiseId in focusAreaIds)
                {
                    _context.Set<MentorshipExpertise>().Add(new MentorshipExpertise
                    {
                        MentorshipId = mentorship.Id,
                        ExpertiseId = expertiseId
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Reload with navigation properties
            var dbMentorship = await _context.Mentorship
                .Include(m => m.Mentor)
                .Include(m => m.Mentee)
                .Include(m => m.FocusAreas)
                    .ThenInclude(fa => fa.Expertise)
                .FirstOrDefaultAsync(m => m.Id == mentorship.Id);
            return _mapper.Map<Mentorship>(dbMentorship);
        }
        catch
        {
            return null;
        }
    }
}