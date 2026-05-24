// File: MoreSpeakers.Data/SectorDataStore.cs

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Data;

public partial class SectorDataStore : ISectorDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SectorDataStore> _logger;

    public SectorDataStore(MoreSpeakersDbContext context, IMapper mapper, ILogger<SectorDataStore> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Sector>> GetAsync(int primaryKey)
    {
        var entity = await _context.Sectors.AsNoTracking().FirstOrDefaultAsync(s => s.Id == primaryKey);
        if (entity is null)
        {
            return Failure<Sector>("sector.not-found", $"Sector {primaryKey} was not found.");
        }

        return Result.Success(_mapper.Map<Sector>(entity));
    }

    public async Task<Result<List<Sector>>> GetAllAsync()
        => await GetAllSectorsAsync(active: TriState.True);

    public async Task<Result<Sector>> GetSectorWithRelationshipsAsync(int id)
    {
        var sector = await _context.Sectors.AsNoTracking()
            .Include(s => s.ExpertiseCategories).FirstOrDefaultAsync(s => s.Id == id);
        if (sector is null)
        {
            return Failure<Sector>("sector.not-found", $"Sector {id} was not found.");
        }

        return Result.Success(_mapper.Map<Sector>(sector));
    }

    public async Task<Result<List<Sector>>> GetAllSectorsAsync(TriState active = TriState.True, string? searchTerm = "", bool includeCategories = false )
    {
        var query = _context.Sectors.AsNoTracking().AsQueryable();

        query = active switch
        {
            TriState.True => query.Where(s => s.IsActive),
            TriState.False => query.Where(s => !s.IsActive),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => s.Name.Contains(searchTerm));
        }

        if (includeCategories)
        {
            query = query.Include(s => s.ExpertiseCategories);
        }

        var entities = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return Result.Success(_mapper.Map<List<Sector>>(entities));
    }

    public async Task<Result<Sector>> SaveAsync(Sector sector)
    {
        try
        {
            var dbEntity = _mapper.Map<Models.Sector>(sector);
            _context.Entry(dbEntity).State = dbEntity.Id == 0 ? EntityState.Added : EntityState.Modified;

            if (await _context.SaveChangesAsync() == 0)
            {
                LogFailedToSaveSector(sector.Name);
                return Failure<Sector>("sector.save.failed", $"Failed to save sector '{sector.Name}'.");
            }

            return Result.Success(_mapper.Map<Sector>(dbEntity));
        }
        catch (DbUpdateException ex)
        {
            LogFailedToSaveSector(ex, sector.Name);
            return Failure<Sector>("sector.save.failed", $"Failed to save sector '{sector.Name}'.", ex);
        }
    }

    public Task<Result> DeleteAsync(Sector entity) => DeleteAsync(entity.Id);

    public async Task<Result> DeleteAsync(int primaryKey)
    {
        try
        {
            var entity = await _context.Sectors
                .Include(s => s.ExpertiseCategories)
                .FirstOrDefaultAsync(s => s.Id == primaryKey);

            if (entity is null)
            {
                return Failure("sector.delete.not-found", $"Sector {primaryKey} was not found.");
            }

            if (entity.ExpertiseCategories.Count != 0)
            {
                return Failure("sector.delete.has-categories", $"Sector {primaryKey} cannot be deleted while expertise categories exist.");
            }

            _context.Sectors.Remove(entity);

            if (await _context.SaveChangesAsync() == 0)
            {
                return Failure("sector.delete.failed", $"Failed to delete sector '{entity.Name}'.");
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            LogFailedToDeleteSector(ex, primaryKey.ToString());
            return Failure("sector.delete.failed", $"Failed to delete sector {primaryKey}.", ex);
        }
    }

    private static Result Failure(string code, string message, Exception? exception = null) =>
        Result.Failure(new Error(code, message, exception));

    private static Result<T> Failure<T>(string code, string message, Exception? exception = null) =>
        Result.Failure<T>(new Error(code, message, exception));
}