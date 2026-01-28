using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Data;

public partial class ExpertiseDataStore : IExpertiseDataStore
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
        var expertise = await _context.Expertise.AsNoTracking().FirstOrDefaultAsync(e => e.Id == primaryKey);
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
            LogFailedToDeleteExpertise(ex, expertise.Name);
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

            LogFailedToSaveExpertise(expertise.Name);
        }
        catch (Exception ex)
        {
            LogFailedToSaveTheExpertiseNameName(ex, expertise.Name);
        }

        throw new ApplicationException("Failed to save the expertise");
    }

    public async Task<List<Expertise>> GetAllAsync()
    {
        var expertises = await _context.Expertise
            .Include(e => e.ExpertiseCategory)
            .OrderBy(e => e.ExpertiseCategory != null ? e.ExpertiseCategory.Name : string.Empty)
            .ThenBy(e => e.Name)
            .ToListAsync();
        return _mapper.Map<List<Expertise>>(expertises);
    }

    public async Task<List<Expertise>> GetAllExpertisesAsync(TriState active = TriState.True, string? searchTerm = "")
    {
        var query = _context.Expertise
            .Include(e => e.ExpertiseCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(e => e.Name.Contains(searchTerm));
        }

        query = active switch
        {
            TriState.True => query.Where(e => e.IsActive),
            TriState.False => query.Where(e => !e.IsActive),
            _ => query
        };

        var entities = await query.OrderBy(e => e.Name).AsNoTracking().ToListAsync();
        return _mapper.Map<List<Expertise>>(entities);
        
    }

    public async Task<int> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 0)
    {
        var expertise =
            new Data.Models.Expertise { Name = name, Description = description, CreatedDate = DateTime.UtcNow, ExpertiseCategoryId = expertiseCategoryId, IsActive = true };
        _context.Expertise.Add(expertise);

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return expertise.Id;
            }

            LogFailedToCreateExpertise(name);
            return 0;
        }
        catch (Exception ex)
        {
            LogFailedToCreateExpertise(ex, name);
        }

        return 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var entity = await _context.Expertise.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
        {
            return true;
        }

        if (!entity.IsActive)
        {
            return true;
        }

        entity.IsActive = false;
        try
        {
            return await _context.SaveChangesAsync() != 0;
        }
        catch (Exception ex)
        {
            LogFailedToSoftDeleteExpertise(ex, id);
            return false;
        }
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
        var entity = await _context.ExpertiseCategory
            .Include(c => c.Sector)
            .AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
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

            LogFailedToSaveExpertiseCategory(category.Name);
        }
        catch (Exception ex)
        {
            LogFailedToSaveExpertiseCategory(ex, category.Name);
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

        var hasExpertises = entity.Expertises.Count != 0;
        if (hasExpertises)
        {
            LogAttemptedToDeleteCategoryWithExpertises(id);
            return false;
        }

        _context.ExpertiseCategory.Remove(entity);
        try
        {
            return await _context.SaveChangesAsync() != 0;
        }
        catch (Exception ex)
        {
            LogFailedToDeleteExpertiseCategory(ex, entity.Name);
            return false;
        }
    }

    public async Task<List<ExpertiseCategory>> GetAllCategoriesAsync(TriState active = TriState.True, string? searchTerm = "")
    {
        var query = _context.ExpertiseCategory
            .Include(c => c.Expertises)
            .Include(c=> c.Sector)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm));
        }

        query = active switch
        {
            TriState.True => query.Where(c => c.IsActive),
            TriState.False => query.Where(c => !c.IsActive),
            _ => query
        };

        var entities = await query.OrderBy(c => c.Name).AsNoTracking().ToListAsync();
        return _mapper.Map<List<ExpertiseCategory>>(entities);
    }

    public async Task<List<ExpertiseCategory>> GetAllActiveCategoriesForSector(int sectorId)
    {
        var categories = await _context.ExpertiseCategory
            .Where(c => c.SectorId == sectorId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return _mapper.Map<List<ExpertiseCategory>>(categories);
    }

    public async Task<List<Expertise>> GetBySectorIdAsync(int sectorFilter)
    {
        var expertises = await _context.Expertise
            .Include(e => e.ExpertiseCategory)
            .Where(e => e.ExpertiseCategory != null && e.ExpertiseCategory.SectorId == sectorFilter && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
        return _mapper.Map<List<Expertise>>(expertises);
    }
}