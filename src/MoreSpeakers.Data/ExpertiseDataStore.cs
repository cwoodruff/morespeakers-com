using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

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

    public async Task<Result<Expertise>> GetAsync(int primaryKey)
    {
        try
        {
            var expertise = await _context.Expertise.AsNoTracking().FirstOrDefaultAsync(e => e.Id == primaryKey);
            if (expertise is null)
            {
                return Failure<Expertise>("expertise.not-found", $"Expertise {primaryKey} was not found.");
            }

            return Result.Success(_mapper.Map<Expertise>(expertise));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<Expertise>("expertise.get.mapping-failed", $"Failed to map expertise {primaryKey}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<Expertise>("expertise.get.failed", $"Failed to load expertise {primaryKey}.", ex);
        }
    }

    public Task<Result> DeleteAsync(Expertise entity) => DeleteAsync(entity.Id);

    public async Task<Result> DeleteAsync(int primaryKey)
    {
        try
        {
            var expertise = await _context.Expertise
                .Include(e => e.UserExpertise)
                .FirstOrDefaultAsync(e => e.Id == primaryKey);

            if (expertise is null)
            {
                return Failure("expertise.delete.not-found", $"Expertise {primaryKey} was not found.");
            }

            foreach (var userExpertise in expertise.UserExpertise)
            {
                _context.UserExpertise.Remove(userExpertise);
            }

            _context.Expertise.Remove(expertise);
            if (await _context.SaveChangesAsync() == 0)
            {
                return Failure("expertise.delete.failed", $"Failed to delete expertise '{expertise.Name}'.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToDeleteExpertise(ex, primaryKey.ToString());
            return Failure("expertise.delete.failed", $"Failed to delete expertise {primaryKey}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            LogFailedToDeleteExpertise(ex, primaryKey.ToString());
            return Failure("expertise.delete.failed", $"Failed to delete expertise {primaryKey}.", ex);
        }
    }

    public async Task<Result<Expertise>> SaveAsync(Expertise expertise)
    {
        try
        {
            var dbExpertise = _mapper.Map<Models.Expertise>(expertise);
            _context.Entry(dbExpertise).State = dbExpertise.Id == 0 ? EntityState.Added : EntityState.Modified;

            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToSaveExpertise(expertise.Name);
                return Failure<Expertise>("expertise.save.failed", $"Failed to save expertise '{expertise.Name}'.");
            }

            return Result.Success(_mapper.Map<Expertise>(dbExpertise));
        }
        catch (AutoMapperMappingException ex)
        {
            LogFailedToSaveTheExpertiseNameName(ex, expertise.Name);
            return Failure<Expertise>("expertise.save.mapping-failed", $"Failed to map expertise '{expertise.Name}'.", ex);
        }
        catch (DbUpdateException ex)
        {
            LogFailedToSaveTheExpertiseNameName(ex, expertise.Name);
            return Failure<Expertise>("expertise.save.failed", $"Failed to save expertise '{expertise.Name}'.", ex);
        }
        catch (InvalidOperationException ex)
        {
            LogFailedToSaveTheExpertiseNameName(ex, expertise.Name);
            return Failure<Expertise>("expertise.save.failed", $"Failed to save expertise '{expertise.Name}'.", ex);
        }
    }

    public async Task<Result<List<Expertise>>> GetAllAsync()
    {
        try
        {
            var expertises = await _context.Expertise
                .Include(e => e.ExpertiseCategory)
                .OrderBy(e => e.ExpertiseCategory != null ? e.ExpertiseCategory.Name : string.Empty)
                .ThenBy(e => e.Name)
                .ToListAsync();

            return Result.Success(_mapper.Map<List<Expertise>>(expertises));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<List<Expertise>>("expertise.list.mapping-failed", "Failed to map the expertise list.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<List<Expertise>>("expertise.list.failed", "Failed to load the expertise list.", ex);
        }
    }

    public async Task<Result<List<Expertise>>> GetAllExpertisesAsync(
        MoreSpeakers.Domain.Models.AdminUsers.TriState active = MoreSpeakers.Domain.Models.AdminUsers.TriState.True,
        string? searchTerm = "")
    {
        try
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
            MoreSpeakers.Domain.Models.AdminUsers.TriState.True => query.Where(e => e.IsActive),
            MoreSpeakers.Domain.Models.AdminUsers.TriState.False => query.Where(e => !e.IsActive),
            _ => query
        };

            var entities = await query.OrderBy(e => e.Name).AsNoTracking().ToListAsync();
            return Result.Success(_mapper.Map<List<Expertise>>(entities));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<List<Expertise>>("expertise.filtered-list.mapping-failed", "Failed to map the filtered expertise list.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<List<Expertise>>("expertise.filtered-list.failed", "Failed to load the filtered expertise list.", ex);
        }
    }

    public async Task<Result<int>> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 0)
    {
        var expertise = new Expertise
        {
            Name = name,
            Description = description,
            CreatedDate = DateTime.UtcNow,
            ExpertiseCategoryId = expertiseCategoryId,
            IsActive = true
        };

        var saveResult = await SaveAsync(expertise);
        return saveResult.IsSuccess
            ? Result.Success(saveResult.Value.Id)
            : Result.Failure<int>(saveResult.Error);
    }

    public async Task<Result> SoftDeleteAsync(int id)
    {
        try
        {
            var entity = await _context.Expertise.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null)
            {
                return Failure("expertise.soft-delete.not-found", $"Expertise {id} was not found.");
            }

            if (!entity.IsActive)
            {
                return Failure("expertise.soft-delete.inactive", $"Expertise {id} is already inactive.");
            }

            entity.IsActive = false;
            if (await _context.SaveChangesAsync() == 0)
            {
                return Failure("expertise.soft-delete.failed", $"Failed to deactivate expertise {id}.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToSoftDeleteExpertise(ex, id);
            return Failure("expertise.soft-delete.failed", $"Failed to deactivate expertise {id}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            LogFailedToSoftDeleteExpertise(ex, id);
            return Failure("expertise.soft-delete.failed", $"Failed to deactivate expertise {id}.", ex);
        }
    }

    public async Task<Result<IEnumerable<Expertise>>> GetPopularExpertiseAsync(int count = 10)
    {
        try
        {
            var expertises = await _context.Expertise
                .Include(e => e.UserExpertise)
                .OrderByDescending(e => e.UserExpertise.Count)
                .Take(count)
                .ToListAsync();

            return Result.Success<IEnumerable<Expertise>>(_mapper.Map<List<Expertise>>(expertises));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<IEnumerable<Expertise>>("expertise.popular.mapping-failed", "Failed to map popular expertise results.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<IEnumerable<Expertise>>("expertise.popular.failed", "Failed to load popular expertise results.", ex);
        }
    }

    public async Task<Result<bool>> DoesExpertiseWithNameExistsAsync(string name)
    {
        try
        {
            var normalizedName = name.Trim().ToLowerInvariant();
            var exists = await _context.Expertise.AnyAsync(e => e.Name.ToLower().Trim() == normalizedName);
            return Result.Success(exists);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<bool>("expertise.exists.failed", $"Failed to verify whether expertise '{name}' exists.", ex);
        }
    }

    public async Task<Result<IEnumerable<Expertise>>> FuzzySearchForExistingExpertise(string name, int count = 3)
    {
        try
        {
            var trimmedName = $"%{name.Trim()}%";
            var expertises = await _context.Expertise
                .Where(e =>
                    EF.Functions.Like(e.Name, trimmedName.ToLower()) ||
                    EF.Functions.Like(e.Description!, trimmedName.ToLower()))
                .Take(count)
                .ToListAsync();

            return Result.Success<IEnumerable<Expertise>>(_mapper.Map<List<Expertise>>(expertises));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<IEnumerable<Expertise>>("expertise.search.mapping-failed", $"Failed to map expertise suggestions for '{name}'.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<IEnumerable<Expertise>>("expertise.search.failed", $"Failed to search expertise suggestions for '{name}'.", ex);
        }
    }

    public async Task<Result<List<Expertise>>> GetByCategoryIdAsync(int categoryId)
    {
        try
        {
            var expertises = await _context.Expertise
                .Where(e => e.ExpertiseCategoryId == categoryId)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return Result.Success(_mapper.Map<List<Expertise>>(expertises));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<List<Expertise>>("expertise.by-category.mapping-failed", $"Failed to map expertise for category {categoryId}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<List<Expertise>>("expertise.by-category.failed", $"Failed to load expertise for category {categoryId}.", ex);
        }
    }

    public async Task<Result<ExpertiseCategory>> GetCategoryAsync(int id)
    {
        try
        {
            var entity = await _context.ExpertiseCategory
                .Include(c => c.Sector)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity is null)
            {
                return Failure<ExpertiseCategory>("expertise-category.not-found", $"Expertise category {id} was not found.");
            }

            return Result.Success(_mapper.Map<ExpertiseCategory>(entity));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<ExpertiseCategory>("expertise-category.get.mapping-failed", $"Failed to map expertise category {id}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<ExpertiseCategory>("expertise-category.get.failed", $"Failed to load expertise category {id}.", ex);
        }
    }

    public async Task<Result<ExpertiseCategory>> SaveCategoryAsync(ExpertiseCategory category)
    {
        try
        {
            var dbEntity = _mapper.Map<Models.ExpertiseCategory>(category);
            _context.Entry(dbEntity).State = dbEntity.Id == 0 ? EntityState.Added : EntityState.Modified;

            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToSaveExpertiseCategory(category.Name);
                return Failure<ExpertiseCategory>("expertise-category.save.failed", $"Failed to save expertise category '{category.Name}'.");
            }

            return Result.Success(_mapper.Map<ExpertiseCategory>(dbEntity));
        }
        catch (AutoMapperMappingException ex)
        {
            LogFailedToSaveExpertiseCategory(ex, category.Name);
            return Failure<ExpertiseCategory>("expertise-category.save.mapping-failed", $"Failed to map expertise category '{category.Name}'.", ex);
        }
        catch (DbUpdateException ex)
        {
            LogFailedToSaveExpertiseCategory(ex, category.Name);
            return Failure<ExpertiseCategory>("expertise-category.save.failed", $"Failed to save expertise category '{category.Name}'.", ex);
        }
        catch (InvalidOperationException ex)
        {
            LogFailedToSaveExpertiseCategory(ex, category.Name);
            return Failure<ExpertiseCategory>("expertise-category.save.failed", $"Failed to save expertise category '{category.Name}'.", ex);
        }
    }

    public async Task<Result> DeleteCategoryAsync(int id)
    {
        try
        {
            var entity = await _context.ExpertiseCategory
                .Include(c => c.Expertises)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity is null)
            {
                return Failure("expertise-category.delete.not-found", $"Expertise category {id} was not found.");
            }

            if (entity.Expertises.Count != 0)
            {
                LogAttemptedToDeleteCategoryWithExpertises(id);
                return Failure("expertise-category.delete.has-expertises", $"Expertise category {id} cannot be deleted while expertises exist.");
            }

            _context.ExpertiseCategory.Remove(entity);
            if (await _context.SaveChangesAsync() == 0)
            {
                return Failure("expertise-category.delete.failed", $"Failed to delete expertise category '{entity.Name}'.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToDeleteExpertiseCategory(ex, id.ToString());
            return Failure("expertise-category.delete.failed", $"Failed to delete expertise category {id}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            LogFailedToDeleteExpertiseCategory(ex, id.ToString());
            return Failure("expertise-category.delete.failed", $"Failed to delete expertise category {id}.", ex);
        }
    }

    public async Task<Result<List<ExpertiseCategory>>> GetAllCategoriesAsync(
        MoreSpeakers.Domain.Models.AdminUsers.TriState active = MoreSpeakers.Domain.Models.AdminUsers.TriState.True,
        string? searchTerm = "")
    {
        try
        {
            var query = _context.ExpertiseCategory
                .Include(c => c.Expertises)
                .Include(c => c.Sector)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }

            query = active switch
            {
            MoreSpeakers.Domain.Models.AdminUsers.TriState.True => query.Where(c => c.IsActive),
            MoreSpeakers.Domain.Models.AdminUsers.TriState.False => query.Where(c => !c.IsActive),
            _ => query
        };

            var entities = await query.OrderBy(c => c.Name).AsNoTracking().ToListAsync();
            return Result.Success(_mapper.Map<List<ExpertiseCategory>>(entities));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<List<ExpertiseCategory>>("expertise-category.list.mapping-failed", "Failed to map expertise categories.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<List<ExpertiseCategory>>("expertise-category.list.failed", "Failed to load expertise categories.", ex);
        }
    }

    public async Task<Result<List<ExpertiseCategory>>> GetAllActiveCategoriesForSector(int sectorId)
    {
        try
        {
            var categories = await _context.ExpertiseCategory
                .Where(c => c.SectorId == sectorId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Result.Success(_mapper.Map<List<ExpertiseCategory>>(categories));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<List<ExpertiseCategory>>("expertise-category.by-sector.mapping-failed", $"Failed to map expertise categories for sector {sectorId}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<List<ExpertiseCategory>>("expertise-category.by-sector.failed", $"Failed to load expertise categories for sector {sectorId}.", ex);
        }
    }

    public async Task<Result<List<Expertise>>> GetBySectorIdAsync(int sectorFilter)
    {
        try
        {
            var expertises = await _context.Expertise
                .Include(e => e.ExpertiseCategory)
                .Where(e => e.ExpertiseCategory != null && e.ExpertiseCategory.SectorId == sectorFilter && e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return Result.Success(_mapper.Map<List<Expertise>>(expertises));
        }
        catch (AutoMapperMappingException ex)
        {
            return Failure<List<Expertise>>("expertise.by-sector.mapping-failed", $"Failed to map expertise for sector {sectorFilter}.", ex);
        }
        catch (InvalidOperationException ex)
        {
            return Failure<List<Expertise>>("expertise.by-sector.failed", $"Failed to load expertise for sector {sectorFilter}.", ex);
        }
    }

    private static Result Failure(string code, string message, Exception? exception = null) =>
        Result.Failure(new Error(code, message, exception));

    private static Result<T> Failure<T>(string code, string message, Exception? exception = null) =>
        Result.Failure<T>(new Error(code, message, exception));
}

