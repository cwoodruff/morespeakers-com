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
        throw new NotImplementedException();
    }

    public async Task<bool> CreeateMentorshipRequestAsync(Mentorship mentorship)
    {
        throw new NotImplementedException();
    }
}