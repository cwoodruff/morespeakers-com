using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

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

    public async Task<EmailTemplate?> GetAsync(string primaryKey)
    {
        var entity = await _context.EmailTemplates.AsNoTracking().FirstOrDefaultAsync(e => e.Location == primaryKey);
        return _mapper.Map<EmailTemplate?>(entity);
    }

    public async Task<bool> DeleteAsync(string primaryKey)
    {
        var entity = await _context.EmailTemplates.FirstOrDefaultAsync(e => e.Location == primaryKey);
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
            _logger.LogError(ex, "Failed to delete email template with location {Location}", primaryKey);
            return false;
        }
    }

    public async Task<EmailTemplate> SaveAsync(EmailTemplate entity)
    {
        var dbEntity = _mapper.Map<Models.EmailTemplate>(entity);
        var existing = await _context.EmailTemplates.AnyAsync(e => e.Location == dbEntity.Location);
        
        _context.Entry(dbEntity).State = existing ? EntityState.Modified : EntityState.Added;

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
            _logger.LogError(ex, "Failed to save email template with location {Location}", entity.Location);
        }

        throw new ApplicationException($"Failed to save email template with location {entity.Location}");
    }

    public async Task<List<EmailTemplate>> GetAllAsync()
    {
        var entities = await _context.EmailTemplates.AsNoTracking().ToListAsync();
        return _mapper.Map<List<EmailTemplate>>(entities);
    }

    public async Task<bool> DeleteAsync(EmailTemplate entity)
    {
        return await DeleteAsync(entity.Location);
    }

    public async Task<EmailTemplate?> GetByLocationAsync(string location)
    {
        return await GetAsync(location);
    }
}