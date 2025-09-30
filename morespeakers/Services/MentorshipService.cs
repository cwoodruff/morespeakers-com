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
        return await _context.Mentorships
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.MentorId == mentorId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mentorship>> GetMentorshipsForNewSpeakerAsync(Guid newSpeakerId)
    {
        return await _context.Mentorships
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.MenteeId == newSpeakerId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<Mentorship?> GetMentorshipByIdAsync(Guid id)
    {
        return await _context.Mentorships
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> RequestMentorshipAsync(Guid newSpeakerId, Guid mentorId, string? notes = null)
    {
        try
        {
            // Check if there's already a pending or active mentorship between these users
            var existingMentorship = await _context.Mentorships
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

            _context.Mentorships.Add(mentorship);
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
            var mentorship = await _context.Mentorships.FindAsync(mentorshipId);
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
            var mentorship = await _context.Mentorships.FindAsync(mentorshipId);
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
            var mentorship = await _context.Mentorships.FindAsync(mentorshipId);
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
            var mentorship = await _context.Mentorships.FindAsync(mentorshipId);
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
        return await _context.Mentorships
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.Status == MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mentorship>> GetActiveMentorshipsAsync()
    {
        return await _context.Mentorships
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Where(m => m.Status == MentorshipStatus.Active)
            .OrderByDescending(m => m.StartedAt)
            .ToListAsync();
    }
}