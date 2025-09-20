using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;

namespace morespeakers.Services;

// Mentorship Service Implementation
public class MentorshipService(ApplicationDbContext context) : IMentorshipService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Mentorship>> GetMentorshipsForMentorAsync(Guid mentorId)
    {
        return await _context.Mentorships
            .Include(m => m.NewSpeaker)
            .Include(m => m.Mentor)
            .Where(m => m.MentorId == mentorId)
            .OrderByDescending(m => m.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mentorship>> GetMentorshipsForNewSpeakerAsync(Guid newSpeakerId)
    {
        return await _context.Mentorships
            .Include(m => m.NewSpeaker)
            .Include(m => m.Mentor)
            .Where(m => m.NewSpeakerId == newSpeakerId)
            .OrderByDescending(m => m.RequestDate)
            .ToListAsync();
    }

    public async Task<Mentorship?> GetMentorshipByIdAsync(Guid id)
    {
        return await _context.Mentorships
            .Include(m => m.NewSpeaker)
            .Include(m => m.Mentor)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> RequestMentorshipAsync(Guid newSpeakerId, Guid mentorId, string? notes = null)
    {
        try
        {
            // Check if mentorship already exists
            var existing = await _context.Mentorships
                .FirstOrDefaultAsync(m => m.NewSpeakerId == newSpeakerId && m.MentorId == mentorId && m.Status != "Cancelled");

            if (existing != null)
                return false;

            var mentorship = new Mentorship
            {
                NewSpeakerId = newSpeakerId,
                MentorId = mentorId,
                Status = "Pending",
                Notes = notes
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
            if (mentorship != null && mentorship.Status == "Pending")
            {
                mentorship.Status = "Active";
                mentorship.AcceptedDate = DateTime.UtcNow;
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

    public async Task<bool> CompleteMentorshipAsync(Guid mentorshipId, string? notes = null)
    {
        try
        {
            var mentorship = await _context.Mentorships.FindAsync(mentorshipId);
            if (mentorship != null && mentorship.Status == "Active")
            {
                mentorship.Status = "Completed";
                mentorship.CompletedDate = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    mentorship.Notes = string.IsNullOrWhiteSpace(mentorship.Notes) 
                        ? notes 
                        : $"{mentorship.Notes}\n\nCompletion Notes: {notes}";
                }
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

    public async Task<bool> CancelMentorshipAsync(Guid mentorshipId, string? reason = null)
    {
        try
        {
            var mentorship = await _context.Mentorships.FindAsync(mentorshipId);
            if (mentorship != null && (mentorship.Status == "Pending" || mentorship.Status == "Active"))
            {
                mentorship.Status = "Cancelled";
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    mentorship.Notes = string.IsNullOrWhiteSpace(mentorship.Notes) 
                        ? $"Cancelled: {reason}" 
                        : $"{mentorship.Notes}\n\nCancellation Reason: {reason}";
                }
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

    public async Task<bool> UpdateMentorshipNotesAsync(Guid mentorshipId, string notes)
    {
        try
        {
            var mentorship = await _context.Mentorships.FindAsync(mentorshipId);
            if (mentorship != null)
            {
                mentorship.Notes = notes;
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

    public async Task<IEnumerable<Mentorship>> GetPendingMentorshipsAsync()
    {
        return await _context.Mentorships
            .Include(m => m.NewSpeaker)
            .Include(m => m.Mentor)
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mentorship>> GetActiveMentorshipsAsync()
    {
        return await _context.Mentorships
            .Include(m => m.NewSpeaker)
            .Include(m => m.Mentor)
            .Where(m => m.Status == "Active")
            .OrderBy(m => m.AcceptedDate)
            .ToListAsync();
    }
}