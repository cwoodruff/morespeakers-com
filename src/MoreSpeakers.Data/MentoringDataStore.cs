using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

using Mentorship = MoreSpeakers.Domain.Models.Mentorship;

namespace MoreSpeakers.Data;

public class MentoringDataStore: IMentoringDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MentoringDataStore> _logger;

    public MentoringDataStore(MoreSpeakersDbContext context, IMapper mapper,  ILogger<MentoringDataStore> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Mentorship?> GetAsync(Guid primaryKey)
    {
        var mentorship = await _context.Mentorship.FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<Mentorship?>(mentorship);
    }

    public async Task<Mentorship> SaveAsync(Mentorship mentorship)
    {
        var dbMentorship = _mapper.Map<Models.Mentorship>(mentorship);
        _context.Entry(dbMentorship).State = dbMentorship.Id == Guid.Empty ? EntityState.Added : EntityState.Modified;

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<Mentorship>(dbMentorship);
            }
            _logger.LogError("Failed to save the mentorship. Id: '{Id}'", mentorship.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the mentorship. Id: '{Id}'", mentorship.Id);
        }

        throw new ApplicationException("Failed to save the mentorship.");
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

        try
        {
            var result = await _context.SaveChangesAsync() !=0;
            if (result)
            {
                return true;
            }
            _logger.LogError("Failed to delete mentorship with id: '{Id}'", primaryKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete mentorship with id: '{Id}'", primaryKey);
        }
        return false;
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

        try
        {
            var created = await _context.SaveChangesAsync();
            if (created == 0)
            {
                _logger.LogError("Failed to create mentorship request. MentorId: '{MentorId}', MenteeId: '{MenteeId}'",
                    mentorship.MentorId, mentorship.MenteeId);
                return false;
            }

            foreach (var expertiseId in expertiseIds)
            {
                await _context.UserExpertise.AddAsync(new Data.Models.UserExpertise
                {
                    UserId = mentorship.MentorId, ExpertiseId = expertiseId
                });
            }

            var expertiseResult = await _context.SaveChangesAsync() != 0;
            if (!expertiseResult)
            {
                _logger.LogError("Failed to create mentorship request - Adding expertises. MentorId: '{MentorId}', MenteeId: '{MenteeId}'",
                    mentorship.MentorId, mentorship.MenteeId);
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create mentorship request. MentorId: '{MentorId}', MenteeId: '{MenteeId}'",
                mentorship.MentorId, mentorship.MenteeId);
        }
        return false;
    }

    public async Task<Mentorship?> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null)
    {
        var mentorship = await _context.Mentorship
            .Include(m => m.Mentor)
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

        try
        {
            var saved = await _context.SaveChangesAsync();
            if (saved != 0)
            {
                return _mapper.Map<Mentorship>(mentorship);
            }

            _logger.LogError("Failed to respond to mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'", mentorshipId, userId);
            return null;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to respond to mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'", mentorshipId, userId);
            return null;
        }
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
            .ThenInclude(m => m.SpeakerType)
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
            .ThenInclude(m => m.SpeakerType)
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

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return true;
            }
            _logger.LogError("Failed to cancel mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'", mentorshipId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'", mentorshipId, userId);
        }
        return false;
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

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return true;
            }
            _logger.LogError("Failed to complete mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'", mentorshipId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete mentorship request. MentorshipId: '{MentorshipId}', UserId: '{UserId}'", mentorshipId, userId);
        }
        return false;
    }

    public async Task<List<User>> GetMentorsExceptForUserAsync(Guid userId, MentorshipType mentorshipType, List<string>? expertiseNames, bool? availability = true)
    {
        var query = _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.Id != userId && u.SpeakerTypeId == 2); // Need to fix this to be dynamic

        if (expertiseNames is not null && expertiseNames.Count > 0)
        {
            var expertiseIds = await _context.Expertise
                .Where(e => expertiseNames.Contains(e.Name))
                .Select(e => e.Id)
                .ToListAsync();

            // User must have all selected expertise areas
            foreach (var expertiseId in expertiseIds)
            {
                query = query.Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId));
            }
        }
        
        // Filter by availability
        if (availability == true)
        {
            query = query.Where(u => u.IsAvailableForMentoring);
        }

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
    
    public async Task<Mentorship?> RequestMentorshipWithDetailsAsync(Guid menteeId, Guid mentorId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency)
    {
        try
        {
            // Check if there's already a pending or active mentorship between these users
            var existingMentorship = await _context.Mentorship
                .FirstOrDefaultAsync(m =>
                    ((m.MenteeId == menteeId && m.MentorId == mentorId) ||
                     (m.MenteeId == mentorId && m.MentorId == menteeId)) &&
                    (m.Status == Models.MentorshipStatus.Pending || m.Status == Models.MentorshipStatus.Active));

            if (existingMentorship != null)
            {
                _logger.LogWarning("User {UserId} already has a mentorship request pending or active with user {TargetId}", menteeId, mentorId);
                return null;
            }

            var dbMentorshipType = _mapper.Map<Data.Models.MentorshipType>(type);
            
            var mentorship = new Models.Mentorship
            {
                Id = Guid.NewGuid(),
                MentorId = mentorId,
                MenteeId = menteeId,
                Status = Models.MentorshipStatus.Pending,
                Type = dbMentorshipType,
                RequestedAt = DateTime.UtcNow,
                RequestMessage = requestMessage,
                PreferredFrequency = preferredFrequency
            };

            _context.Mentorship.Add(mentorship);
            var mentorshipAddResult = await _context.SaveChangesAsync() != 0;
            {
                if (!mentorshipAddResult)
                {
                    _logger.LogError("Failed to create mentorship request with details. MentorId: '{MentorId}', MenteeId: '{MenteeId}'", mentorId, menteeId);
                    return null;
                }
            }

            // Add focus areas if provided
            if (focusAreaIds != null && focusAreaIds.Any())
            {
                foreach (var expertiseId in focusAreaIds)
                {
                    _context.Set<Data.Models.MentorshipExpertise>().Add(new Data.Models.MentorshipExpertise
                    {
                        MentorshipId = mentorship.Id,
                        ExpertiseId = expertiseId
                    });
                }
                var expertiseResult = await _context.SaveChangesAsync();
                if (expertiseResult == 0)
                {
                    _logger.LogError("Failed to create mentorship request with details - Save Expertises. MentorId: '{MentorId}', MenteeId: '{MenteeId}'", mentorId, menteeId);
                    return null;
                }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create mentorship request with details. MentorId: '{MentorId}', MenteeId: '{MenteeId}'", menteeId, mentorId);
            return null;
        }
    }

    public async Task<Mentorship?> GetMentorshipWithRelationships(Guid mentorshipId)
    {
        var mentorship = await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
                .ThenInclude(fa => fa.Expertise)
            .FirstOrDefaultAsync(m => m.Id == mentorshipId);
        return _mapper.Map<Mentorship>(mentorship);
    }
}