using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Data;

public class ExpertiseDataStore : IExpertiseDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ExpertiseDataStore> _logger;

    public ExpertiseDataStore(MoreSpeakersDbContext context, IMapper mapper, ILogger<ExpertiseDataStore> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Expertise?> GetAsync(int primaryKey)
    {
        var expertise = await _context.Expertise.FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<Expertise?>(expertise);
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

        try
        {
            return await _context.SaveChangesAsync() != 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the expertise. Name: '{Name}'", expertise.Name);
            return false;
        }
    }

    public async Task<Expertise> SaveAsync(Expertise expertise)
    {
        var dbExpertise = _mapper.Map<Models.Expertise>(expertise);
        _context.Entry(dbExpertise).State = dbExpertise.Id == 0 ? EntityState.Added : EntityState.Modified;

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<Expertise>(dbExpertise);
            }

            _logger.LogError("Failed to save the expertise. Name: '{Name}'", expertise.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the expertise. Name: '{Name}'", expertise.Name);
        }

        throw new ApplicationException("Failed to save the expertise");
    }

    public async Task<List<Expertise>> GetAllAsync()
    {
        var expertises = await _context.Expertise
            .Include(e => e.ExpertiseCategory)
            .OrderBy(e => e.ExpertiseCategory.Name)
            .ThenBy(e => e.Name)
            .ToListAsync();
        return _mapper.Map<List<Expertise>>(expertises);
    }

    public async Task<int> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 0)
    {
        var expertise =
            new Data.Models.Expertise { Name = name, Description = description, CreatedDate = DateTime.UtcNow, ExpertiseCategoryId = expertiseCategoryId };
        _context.Expertise.Add(expertise);

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return expertise.Id;
            }

            _logger.LogError("Failed to create the expertise. Name: '{Name}'", name);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create the expertise. Name: '{Name}'", name);
        }

        return 0;
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

    public async Task<bool> DoesExpertiseWithNameExistsAsync(string name)
    {
        var experiences = await _context.Expertise.FirstOrDefaultAsync(e =>
            e.Name.Trim().Equals(name.Trim().ToLower()));
        return experiences != null;
    }

    public async Task<IEnumerable<Expertise>> FuzzySearchForExistingExpertise(string name, int count = 3)
    {
        var trimmedName = $"%{name.Trim()}%";
        var expertises = await _context.Expertise
            .Where(e => 
                EF.Functions.Like(e.Name, trimmedName.ToLower()) ||
                EF.Functions.Like(e.Description, trimmedName.ToLower()))
            .Take(count)
            .ToListAsync();
        
        return _mapper.Map<List<Expertise>>(expertises);   
    }

    public async Task<List<Expertise>> GetByCategoryIdAsync(int categoryId)
    {
        var expertises = await _context.Expertise
            .Where(e => e.ExpertiseCategoryId == categoryId)
            .OrderBy(e => e.Name)
            .ToListAsync();
        return _mapper.Map<List<Expertise>>(expertises);
    }

    // Category operations
    public async Task<ExpertiseCategory?> GetCategoryAsync(int id)
    {
        var entity = await _context.ExpertiseCategory.FirstOrDefaultAsync(c => c.Id == id);
        return _mapper.Map<ExpertiseCategory?>(entity);
    }

    public async Task<ExpertiseCategory> SaveCategoryAsync(ExpertiseCategory category)
    {
        var dbEntity = _mapper.Map<Models.ExpertiseCategory>(category);
        _context.Entry(dbEntity).State = dbEntity.Id == 0 ? EntityState.Added : EntityState.Modified;

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<ExpertiseCategory>(dbEntity);
            }

            _logger.LogError("Failed to save the expertise category. Name: '{Name}'", category.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the expertise category. Name: '{Name}'", category.Name);
        }

        throw new ApplicationException("Failed to save the expertise category");
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var entity = await _context.ExpertiseCategory.Include(c => c.Expertises)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
        {
            return true;
        }

        var hasExpertises = entity.Expertises.Any();
        if (hasExpertises)
        {
            _logger.LogWarning("Attempted to delete category with id {Id} that still has expertises", id);
            return false;
        }

        _context.ExpertiseCategory.Remove(entity);
        try
        {
            return await _context.SaveChangesAsync() != 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the expertise category. Name: '{Name}'", entity.Name);
            return false;
        }
    }

    public async Task<List<ExpertiseCategory>> GetAllCategoriesAsync(bool onlyActive = true)
    {
        var query = _context.ExpertiseCategory.AsQueryable();
        if (onlyActive)
        {
            query = query.Where(c => c.IsActive);
        }
        var entities = await query.OrderBy(c => c.Name).ToListAsync();
        return _mapper.Map<List<ExpertiseCategory>>(entities);
    }
}