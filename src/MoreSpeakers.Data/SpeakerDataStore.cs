using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Data;

public class SpeakerDataStore : ISpeakerDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly Mapper _mapper;

    private SpeakerDataStore(IDatabaseSettings databaseSettings)
    {
        _context = new MoreSpeakersDbContext(databaseSettings);
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        });
        _mapper = new Mapper(mappingConfiguration);
    }
    
    public async Task<User> GetAsync(Guid primaryKey)
    {
        var speaker = await _context.Users.FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<User>(speaker);
    }

    public async Task<User> SaveAsync(User entity)
    {
        var user = _mapper.Map<Models.User>(entity);
        _context.Entry(entity).State = entity.Id == Guid.Empty ? EntityState.Added : EntityState.Modified;

        var result = await _context.SaveChangesAsync() != 0;
        if (result)
        {
            return _mapper.Map<User>(user);
        }

        throw new ApplicationException("Failed to save the user");
    }

    public async Task<List<User>> GetAllAsync()
    {
        var speakers = await _context.Users.ToListAsync();
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
}