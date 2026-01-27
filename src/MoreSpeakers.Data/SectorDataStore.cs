// File: MoreSpeakers.Data/SectorDataStore.cs

using System.Diagnostics;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Data;

public class SectorDataStore : ISectorDataStore
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

    public async Task<Sector?> GetAsync(int primaryKey)
    {
        var entity = await _context.Sectors.AsNoTracking().FirstOrDefaultAsync(s => s.Id == primaryKey);
        return _mapper.Map<Sector?>(entity);
    }

    public async Task<List<Sector>> GetAllAsync()
        => await GetAllSectorsAsync(active: TriState.True);

    public async Task<Sector?> GetSectorWithRelationshipsAsync(int id)
    {
        var sector = await _context.Sectors.AsNoTracking()
            .Include(s => s.ExpertiseCategories).FirstOrDefaultAsync(s => s.Id == id);
        return _mapper.Map<Sector?>(sector);
    }

    public async Task<List<Sector>> GetAllSectorsAsync(TriState active = TriState.True, string? searchTerm = "", bool includeCategories = false )
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

        return _mapper.Map<List<Sector>>(entities);
    }

    public async Task<Sector> SaveAsync(Sector sector)
    {
        var dbEntity = _mapper.Map<Models.Sector>(sector);
        _context.Entry(dbEntity).State = dbEntity.Id == 0 ? EntityState.Added : EntityState.Modified;

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
                return _mapper.Map<Sector>(dbEntity);

            _logger.LogError("Failed to save the sector. Name: '{Name}'", sector.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the sector. Name: '{Name}'", sector.Name);
        }

        throw new ApplicationException("Failed to save the sector");
    }

    public Task<bool> DeleteAsync(Sector entity) => DeleteAsync(entity.Id);

    public async Task<bool> DeleteAsync(int primaryKey)
    {
        var entity = await _context.Sectors
            .Include(s => s.ExpertiseCategories)
            .FirstOrDefaultAsync(s => s.Id == primaryKey);

        if (entity is null)
            return true;

        if (entity.ExpertiseCategories.Count != 0)
            return false;

        _context.Sectors.Remove(entity);

        try
        {
            return await _context.SaveChangesAsync() != 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the sector. Name: '{Name}'", entity.Name);
            return false;
        }
    }
}