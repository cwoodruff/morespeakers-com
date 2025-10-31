using AutoMapper;

using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Data;

public class ExpertiseDataStore : IExpertiseDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly Mapper _mapper;

    public ExpertiseDataStore(MoreSpeakersDbContext context)
    {
        _context = context;
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        });
        _mapper = new Mapper(mappingConfiguration);
    }

    public async Task<Expertise> GetAsync(int primaryKey)
    {
        var expertise = await _context.Expertise.FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<Expertise>(expertise);
    }

    public async Task<Expertise> SaveAsync(Expertise expertise)
    {
        var dbExpertise = _mapper.Map<Models.Expertise>(expertise);
        _context.Entry(dbExpertise).State = dbExpertise.Id == 0 ? EntityState.Added : EntityState.Modified;

        var result = await _context.SaveChangesAsync() != 0;
        if (result)
        {
            return _mapper.Map<Expertise>(dbExpertise);
        }

        throw new ApplicationException("Failed to save the expertise");
    }

    public async Task<List<Expertise>> GetAllAsync()
    {
        var expertises = await _context.Expertise.OrderBy(e => e.Name).ToListAsync();
        return _mapper.Map<List<Expertise>>(expertises);
    }

    public async Task<bool> DeleteAsync(Expertise entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public async Task<bool> DeleteAsync(int primaryKey)
    {
        var expertise = await _context.Expertise.Include(e => e.UserExpertise)
            .FirstOrDefaultAsync(e => e.Id == primaryKey);

        if (expertise is null)
        {
            return true;
        }
        
        foreach (var userExpertise in expertise.UserExpertise)
        {
            _context.UserExpertise.Remove(userExpertise);
        }
        _context.Expertise.Remove(expertise);

        return await _context.SaveChangesAsync() != 0;
    }

    public async Task<int> CreateExpertiseAsync(string name, string? description = null)
    {
        var expertise = new Data.Models.Expertise { Name = name, Description = description, CreatedDate = DateTime.UtcNow};
        _context.Expertise.Add(expertise);
        await _context.SaveChangesAsync();
        return expertise.Id;   
    }

    public async Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10)
    {
        var expertises = await _context.Expertise
            .Include(e => e.UserExpertise)
            .OrderByDescending(e => e.UserExpertise.Count)
            .Take(count)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<Expertise>>(expertises);
    }

    public async Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm)
    {
        var expertises = await _context.Expertise
            .Where(e => e.Name.Contains(searchTerm) ||
                        (e.Description != null && e.Description.Contains(searchTerm)))
            .OrderBy(e => e.Name)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<Expertise>>(expertises);
    }

    public async Task<Expertise?> SearchForExpertiseExistsAsync(string name)
    {
        var experiences = await _context.Expertise.FirstOrDefaultAsync(e => e.Name.Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase));
        return _mapper.Map<Expertise>(experiences);   
    }
    
    public async Task<List<Expertise>> FuzzySearchForExistingExpertise(string name, int count = 3)
    {
        var trimmedName = name.Trim();
        var expertises = await _context.Expertise
            .Where(e => e.Name.ToLower().Contains(trimmedName.ToLower()) ||
                        trimmedName.ToLower().Contains(e.Name.ToLower()))
            .Take(count)
            .Select(e => new { e.Id, e.Name })
            .ToListAsync();
        
        return _mapper.Map<List<Expertise>>(expertises.FirstOrDefault());   
    }
}