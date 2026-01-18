using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Data;

public class EmailTemplateDataStore : IEmailTemplateDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EmailTemplateDataStore> _logger;

    public EmailTemplateDataStore(MoreSpeakersDbContext context, IMapper mapper, ILogger<EmailTemplateDataStore> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplate?> GetAsync(int primaryKey)
    {
        var entity = await _context.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(e => e.Id == primaryKey);
        return _mapper.Map<EmailTemplate?>(entity);
    }

    public async Task<bool> DeleteAsync(int primaryKey)
    {
        var entity = await _context.EmailTemplates.FirstOrDefaultAsync(e => e.Id == primaryKey);
        if (entity == null)
        {
            return true;
        }

        _context.EmailTemplates.Remove(entity);
        try
        {
            return await _context.SaveChangesAsync() != 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete email template with id {Id}", primaryKey);
            return false;
        }
    }

    public async Task<EmailTemplate> SaveAsync(EmailTemplate entity)
    {
        var dbEntity = _mapper.Map<Models.EmailTemplate>(entity);
        _context.Entry(dbEntity).State = entity.Id == 0 ? EntityState.Added : EntityState.Modified;

        try
        {
            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<EmailTemplate>(dbEntity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email template with id {Id}", entity.Id);
        }

        throw new ApplicationException($"Failed to save email template with id {entity.Id}");
    }

    public async Task<List<EmailTemplate>> GetAllAsync()
    {
        var entities = await _context.EmailTemplates.AsNoTracking().ToListAsync();
        return _mapper.Map<List<EmailTemplate>>(entities);
    }

    public async Task<List<EmailTemplate>> GetAllTemplatesAsync(TriState active = TriState.Any, string? searchTerm = "")
    {
        var query = _context.EmailTemplates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Location.Contains(searchTerm));
        }

        query = active switch
        {
            TriState.True => query.Where(t => t.IsActive),
            TriState.False => query.Where(t => !t.IsActive),
            _ => query
        };

        var entities = await query.OrderBy(t => t.Location).AsNoTracking().ToListAsync();
        return _mapper.Map<List<EmailTemplate>>(entities);
    }

    public async Task<bool> DeleteAsync(EmailTemplate entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public async Task<EmailTemplate?> GetByLocationAsync(string location)
    {
        var entity = await _context.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(e => e.Location == location);
        return _mapper.Map<EmailTemplate?>(entity);
    }
}