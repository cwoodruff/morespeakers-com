using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;

namespace morespeakers.Services;

public class MentorshipService : IMentorshipService
{
    private readonly ApplicationDbContext _context;

    public MentorshipService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Mentorship>> GetMentorshipsForMentorAsync(Guid mentorId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.MentorId == mentorId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mentorship>> GetMentorshipsForNewSpeakerAsync(Guid newSpeakerId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.MenteeId == newSpeakerId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<Mentorship?> GetMentorshipByIdAsync(Guid id)
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> RequestMentorshipAsync(Guid newSpeakerId, Guid mentorId, string? notes = null)
    {
        try
        {
            // Check if there's already a pending or active mentorship between these users
            var existingMentorship = await _context.Mentorship
                .FirstOrDefaultAsync(m => 
                    m.MenteeId == newSpeakerId && 
                    m.MentorId == mentorId && 
                    (m.Status == MentorshipStatus.Pending || m.Status == MentorshipStatus.Active));

            if (existingMentorship != null)
                return false;

            var mentorship = new Mentorship
            {
                Id = Guid.NewGuid(),
                MentorId = mentorId,
                MenteeId = newSpeakerId,
                Status = MentorshipStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                RequestMessage = notes,
                Type = MentorshipType.NewToExperienced
            };

            _context.Mentorship.Add(mentorship);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AcceptMentorshipAsync(Guid mentorshipId)
    {
        try
        {
            var mentorship = await _context.Mentorship.FindAsync(mentorshipId);
            if (mentorship == null || mentorship.Status != MentorshipStatus.Pending)
                return false;

            mentorship.Status = MentorshipStatus.Active;
            mentorship.StartedAt = DateTime.UtcNow;
            mentorship.ResponsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CompleteMentorshipAsync(Guid mentorshipId, string? notes = null)
    {
        try
        {
            var mentorship = await _context.Mentorship.FindAsync(mentorshipId);
            if (mentorship == null || mentorship.Status != MentorshipStatus.Active)
                return false;

            mentorship.Status = MentorshipStatus.Completed;
            mentorship.CompletedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(notes))
                mentorship.Notes = notes;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CancelMentorshipAsync(Guid mentorshipId, string? reason = null)
    {
        try
        {
            var mentorship = await _context.Mentorship.FindAsync(mentorshipId);
            if (mentorship == null)
                return false;

            mentorship.Status = MentorshipStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
                mentorship.Notes = reason;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateMentorshipNotesAsync(Guid mentorshipId, string notes)
    {
        try
        {
            var mentorship = await _context.Mentorship.FindAsync(mentorshipId);
            if (mentorship == null)
                return false;

            mentorship.Notes = notes;
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Mentorship>> GetPendingMentorshipsAsync()
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.Status == MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mentorship>> GetActiveMentorshipsAsync()
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.Status == MentorshipStatus.Active)
            .OrderByDescending(m => m.StartedAt)
            .ToListAsync();
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
                    (m.Status == MentorshipStatus.Pending || m.Status == MentorshipStatus.Active));

            if (existingMentorship != null)
                return null;

            var mentorship = new Mentorship
            {
                Id = Guid.NewGuid(),
                MentorId = targetId,
                MenteeId = requesterId,
                Status = MentorshipStatus.Pending,
                Type = type,
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
            return await _context.Mentorship
                .Include(m => m.Mentor)
                .Include(m => m.Mentee)
                .Include(m => m.FocusAreas)
                    .ThenInclude(fa => fa.Expertise)
                .FirstOrDefaultAsync(m => m.Id == mentorship.Id);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeclineMentorshipAsync(Guid mentorshipId, string? declineReason)
    {
        try
        {
            var mentorship = await _context.Mentorship.FindAsync(mentorshipId);
            if (mentorship == null || mentorship.Status != MentorshipStatus.Pending)
                return false;

            mentorship.Status = MentorshipStatus.Declined;
            mentorship.ResponsedAt = DateTime.UtcNow;
            mentorship.ResponseMessage = declineReason;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(int incoming, int outgoing)> GetPendingCountsAsync(Guid userId)
    {
        var incoming = await _context.Mentorship
            .CountAsync(m => m.MentorId == userId && m.Status == MentorshipStatus.Pending);

        var outgoing = await _context.Mentorship
            .CountAsync(m => m.MenteeId == userId && m.Status == MentorshipStatus.Pending);

        return (incoming, outgoing);
    }

    public async Task<IEnumerable<Mentorship>> GetMentorshipsForUserAsync(Guid userId, MentorshipStatus? status = null)
    {
        var query = _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
                .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MentorId == userId || m.MenteeId == userId);

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        return await query
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<bool> CanRequestMentorshipAsync(Guid requesterId, Guid targetId)
    {
        // Check if there's already a pending or active mentorship
        var existingMentorship = await _context.Mentorship
            .AnyAsync(m =>
                ((m.MenteeId == requesterId && m.MentorId == targetId) ||
                 (m.MenteeId == targetId && m.MentorId == requesterId)) &&
                (m.Status == MentorshipStatus.Pending || m.Status == MentorshipStatus.Active));

        return !existingMentorship;
    }

    public async Task<IEnumerable<Mentorship>> GetIncomingRequestsAsync(Guid userId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
                .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MentorId == userId && m.Status == MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mentorship>> GetOutgoingRequestsAsync(Guid userId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
                .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MenteeId == userId && m.Status == MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }
}