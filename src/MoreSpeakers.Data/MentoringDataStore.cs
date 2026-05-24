using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

using Mentorship = MoreSpeakers.Domain.Models.Mentorship;

namespace MoreSpeakers.Data;

public partial class MentoringDataStore : IMentoringDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MentoringDataStore> _logger;

    public MentoringDataStore(MoreSpeakersDbContext context, IMapper mapper, ILogger<MentoringDataStore> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Mentorship>> GetAsync(Guid primaryKey)
    {
        var mentorship = await _context.Mentorship.FirstOrDefaultAsync(e => e.Id == primaryKey);
        if (mentorship is null)
        {
            return Failure<Mentorship>("mentorship.not-found", $"Mentorship {primaryKey} was not found.");
        }

        return Result.Success(_mapper.Map<Mentorship>(mentorship));
    }

    public async Task<Result<Mentorship>> SaveAsync(Mentorship mentorship)
    {
        var dbMentorship = _mapper.Map<Models.Mentorship>(mentorship);

        if (dbMentorship.Id != Guid.Empty)
        {
            var tracked = _context.Mentorship.Local.FirstOrDefault(e => e.Id == dbMentorship.Id);
            if (tracked != null)
                _context.Entry(tracked).State = EntityState.Detached;
        }

        var exists = dbMentorship.Id != Guid.Empty && await _context.Mentorship.AnyAsync(e => e.Id == dbMentorship.Id);
        _context.Entry(dbMentorship).State = exists ? EntityState.Modified : EntityState.Added;

        try
        {
            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToSaveMentorship(mentorship.Id);
                return Failure<Mentorship>("mentorship.save-failed", $"Failed to save mentorship {mentorship.Id}.");
            }

            return Result.Success(_mapper.Map<Mentorship>(dbMentorship));
        }
        catch (DbUpdateException ex)
        {
            LogFailedToSaveMentorship(ex, mentorship.Id);
            return Result.Failure<Mentorship>(new Error("mentorship.save-failed", $"Failed to save mentorship {mentorship.Id}.", ex));
        }
    }

    public async Task<Result<List<Mentorship>>> GetAllAsync()
    {
        var mentorships = await _context.Mentorship.ToListAsync();
        return Result.Success(_mapper.Map<List<Mentorship>>(mentorships));
    }

    public Task<Result> DeleteAsync(Mentorship entity) => DeleteAsync(entity.Id);

    public async Task<Result> DeleteAsync(Guid primaryKey)
    {
        var mentorship = await _context.Mentorship.FirstOrDefaultAsync(e => e.Id == primaryKey);
        if (mentorship is null)
        {
            return Failure("mentorship.delete.not-found", $"Mentorship {primaryKey} was not found.");
        }

        _context.Mentorship.Remove(mentorship);

        try
        {
            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToDeleteMentorship(primaryKey);
                return Failure("mentorship.delete.failed", $"Failed to delete mentorship {primaryKey}.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToDeleteMentorship(ex, primaryKey);
            return Result.Failure(new Error("mentorship.delete.failed", $"Failed to delete mentorship {primaryKey}.", ex));
        }
    }

    public async Task<Result<List<Expertise>>> GetSharedExpertisesAsync(User mentor, User mentee)
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

        return Result.Success(_mapper.Map<List<Expertise>>(expertises));
    }

    public async Task<Result<bool>> DoesMentorshipRequestsExistsAsync(User mentor, User mentee)
    {
        var exists = await _context.Mentorship.FirstOrDefaultAsync(m =>
            m.MentorId == mentor.Id &&
            m.MenteeId == mentee.Id &&
            m.Status == Models.MentorshipStatus.Pending);

        return Result.Success(exists is not null);
    }

    public async Task<Result> CreateMentorshipRequestAsync(Mentorship mentorship, List<int> expertiseIds)
    {
        var dbMentorship = _mapper.Map<Data.Models.Mentorship>(mentorship);

        _context.Mentorship.Add(dbMentorship);

        try
        {
            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToCreateMentorshipRequest(mentorship.MentorId, mentorship.MenteeId);
                return Failure("mentorship.request.failed", "Failed to create mentorship request.");
            }

            foreach (var expertiseId in expertiseIds)
            {
                await _context.UserExpertise.AddAsync(new Data.Models.UserExpertise
                {
                    UserId = mentorship.MentorId,
                    ExpertiseId = expertiseId
                });
            }

            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToCreateMentorshipRequestAddingExpertises(mentorship.MentorId, mentorship.MenteeId);
                return Failure("mentorship.request.expertise-save-failed", "Failed to save mentorship request expertises.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToCreateMentorshipRequest(ex, mentorship.MentorId, mentorship.MenteeId);
            return Result.Failure(new Error("mentorship.request.failed", "Failed to create mentorship request.", ex));
        }
    }

    public async Task<Result<Mentorship>> RespondToRequestAsync(Guid mentorshipId, Guid userId, bool accepted, string? message = null)
    {
        var mentorship = await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && m.MentorId == userId);

        if (mentorship is null)
        {
            return Failure<Mentorship>("mentorship.respond.not-found", $"Mentorship {mentorshipId} was not found for user {userId}.");
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
            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToRespondToMentorshipRequest(mentorshipId, userId);
                return Failure<Mentorship>("mentorship.respond.failed", $"Failed to respond to mentorship request {mentorshipId}.");
            }

            return Result.Success(_mapper.Map<Mentorship>(mentorship));
        }
        catch (DbUpdateException ex)
        {
            LogFailedToRespondToMentorshipRequest(ex, mentorshipId, userId);
            return Result.Failure<Mentorship>(new Error("mentorship.respond.failed", $"Failed to respond to mentorship request {mentorshipId}.", ex));
        }
    }

    public async Task<Result<List<Mentorship>>> GetActiveMentorshipsForUserAsync(Guid userId)
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

        return Result.Success(_mapper.Map<List<Mentorship>>(activeMentorships));
    }

    public async Task<Result<(int outboundCount, int inboundCount)>> GetNumberOfMentorshipsPending(Guid userId)
    {
        var outboundCount = await _context.Mentorship.CountAsync(m =>
            m.MenteeId == userId &&
            m.Status == Models.MentorshipStatus.Pending);
        var inboundCount = await _context.Mentorship.CountAsync(m =>
            m.MentorId == userId &&
            m.Status == Models.MentorshipStatus.Pending);

        return Result.Success((outboundCount, inboundCount));
    }

    public async Task<Result<List<Mentorship>>> GetIncomingMentorshipRequests(Guid userId)
    {
        var mentorships = await _context.Mentorship
            .Include(m => m.Mentee)
            .ThenInclude(m => m.SpeakerType)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MentorId == userId && m.Status == Models.MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();

        return Result.Success(_mapper.Map<List<Mentorship>>(mentorships));
    }

    public async Task<Result<List<Mentorship>>> GetOutgoingMentorshipRequests(Guid userId)
    {
        var mentorships = await _context.Mentorship
            .Include(m => m.Mentor)
            .ThenInclude(m => m.SpeakerType)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MenteeId == userId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();

        return Result.Success(_mapper.Map<List<Mentorship>>(mentorships));
    }

    public async Task<Result> CancelMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var mentorship = await _context.Mentorship
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && (m.MenteeId == userId || m.MentorId == userId));

        if (mentorship is null)
        {
            return Failure("mentorship.cancel.not-found", $"Mentorship {mentorshipId} was not found for user {userId}.");
        }

        mentorship.Status = Models.MentorshipStatus.Cancelled;
        mentorship.UpdatedAt = DateTime.UtcNow;
        _context.Mentorship.Remove(mentorship);

        try
        {
            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToCancelMentorshipRequest(mentorshipId, userId);
                return Failure("mentorship.cancel.failed", $"Failed to cancel mentorship request {mentorshipId}.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToCancelMentorshipRequest(ex, mentorshipId, userId);
            return Result.Failure(new Error("mentorship.cancel.failed", $"Failed to cancel mentorship request {mentorshipId}.", ex));
        }
    }

    public async Task<Result> CompleteMentorshipRequestAsync(Guid mentorshipId, Guid userId)
    {
        var mentorship = await _context.Mentorship
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && (m.MenteeId == userId || m.MentorId == userId));

        if (mentorship is null)
        {
            return Failure("mentorship.complete.not-found", $"Mentorship {mentorshipId} was not found for user {userId}.");
        }

        mentorship.Status = Models.MentorshipStatus.Completed;
        mentorship.UpdatedAt = DateTime.UtcNow;
        _context.Mentorship.Remove(mentorship);

        try
        {
            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToCompleteMentorshipRequest(mentorshipId, userId);
                return Failure("mentorship.complete.failed", $"Failed to complete mentorship request {mentorshipId}.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToCompleteMentorshipRequest(ex, mentorshipId, userId);
            return Result.Failure(new Error("mentorship.complete.failed", $"Failed to complete mentorship request {mentorshipId}.", ex));
        }
    }

    public async Task<Result<List<User>>> GetMentorsExceptForUserAsync(Guid userId, MentorshipType mentorshipType, List<string>? expertiseNames, bool? availability = true)
    {
        var query = _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.Id != userId && u.SpeakerTypeId == 2);

        if (expertiseNames is not null && expertiseNames.Count > 0)
        {
            var expertiseIds = await _context.Expertise
                .Where(e => expertiseNames.Contains(e.Name))
                .Select(e => e.Id)
                .ToListAsync();

            foreach (var expertiseId in expertiseIds)
            {
                query = query.Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId));
            }
        }

        if (availability == true)
        {
            query = query.Where(u => u.IsAvailableForMentoring);
        }

        var mentors = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        return Result.Success(_mapper.Map<List<User>>(mentors));
    }

    public async Task<Result<User>> GetMentorAsync(Guid userId)
    {
        var mentor = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (mentor is null)
        {
            return Failure<User>("mentorship.mentor.not-found", $"Mentor {userId} was not found.");
        }

        return Result.Success(_mapper.Map<User>(mentor));
    }

    public async Task<Result<bool>> CanRequestMentorshipAsync(Guid menteeId, Guid mentorId)
    {
        var existingMentorship = await _context.Mentorship
            .AnyAsync(m =>
                ((m.MenteeId == menteeId && m.MentorId == mentorId) ||
                 (m.MenteeId == mentorId && m.MentorId == menteeId)) &&
                (m.Status == Models.MentorshipStatus.Pending || m.Status == Models.MentorshipStatus.Active));

        return Result.Success(!existingMentorship);
    }

    public async Task<Result<Mentorship>> RequestMentorshipWithDetailsAsync(Guid menteeId, Guid mentorId,
        MentorshipType type, string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency)
    {
        var existingMentorship = await _context.Mentorship
            .FirstOrDefaultAsync(m =>
                ((m.MenteeId == menteeId && m.MentorId == mentorId) ||
                 (m.MenteeId == mentorId && m.MentorId == menteeId)) &&
                (m.Status == Models.MentorshipStatus.Pending || m.Status == Models.MentorshipStatus.Active));

        if (existingMentorship is not null)
        {
            LogUserAlreadyHasAMentorshipRequest(menteeId, mentorId);
            return Failure<Mentorship>("mentorship.request.already-exists", "A mentorship request already exists between these users.");
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

        try
        {
            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToCreateMentorshipRequestWithDetails(mentorId, menteeId);
                return Failure<Mentorship>("mentorship.request.failed", "Failed to create mentorship request.");
            }

            if (focusAreaIds is not null && focusAreaIds.Count != 0)
            {
                foreach (var expertiseId in focusAreaIds)
                {
                    _context.Set<Data.Models.MentorshipExpertise>().Add(new Data.Models.MentorshipExpertise
                    {
                        MentorshipId = mentorship.Id,
                        ExpertiseId = expertiseId
                    });
                }

                if (await _context.SaveChangesAsync() == 0)
                {
                    LogFailedToCreateMentorshipRequestDuringSaveExpertise(mentorId, menteeId);
                    return Failure<Mentorship>("mentorship.request.expertise-save-failed", "Failed to save mentorship request focus areas.");
                }
            }
        }
        catch (DbUpdateException ex)
        {
            LogFailedToCreateMentorshipRequestWithDetails(ex, menteeId, mentorId);
            return Result.Failure<Mentorship>(new Error("mentorship.request.failed", "Failed to create mentorship request.", ex));
        }

        var dbMentorship = await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .FirstOrDefaultAsync(m => m.Id == mentorship.Id);

        if (dbMentorship is null)
        {
            return Failure<Mentorship>("mentorship.not-found", $"Mentorship {mentorship.Id} was not found.");
        }

        return Result.Success(_mapper.Map<Mentorship>(dbMentorship));
    }

    public async Task<Result<Mentorship>> GetMentorshipWithRelationships(Guid mentorshipId)
    {
        var mentorship = await _context.Mentorship
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .FirstOrDefaultAsync(m => m.Id == mentorshipId);

        if (mentorship is null)
        {
            return Failure<Mentorship>("mentorship.not-found", $"Mentorship {mentorshipId} was not found.");
        }

        return Result.Success(_mapper.Map<Mentorship>(mentorship));
    }

    private static Result<T> Failure<T>(string code, string message) => Result.Failure<T>(new Error(code, message));
    private static Result Failure(string code, string message) => Result.Failure(new Error(code, message));
}
