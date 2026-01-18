using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Managers;

public class EmailTemplateManager : IEmailTemplateManager
{
    private readonly IEmailTemplateDataStore _dataStore;
    private readonly ILogger<EmailTemplateManager> _logger;

    public EmailTemplateManager(IEmailTemplateDataStore dataStore, ILogger<EmailTemplateManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }

    public async Task<EmailTemplate?> GetAsync(int id)
    {
        return await _dataStore.GetAsync(id);
    }

    public async Task<EmailTemplate?> GetByLocationAsync(string location)
    {
        return await _dataStore.GetByLocationAsync(location);
    }

    public async Task<EmailTemplate> SaveAsync(EmailTemplate emailTemplate)
    {
        return await _dataStore.SaveAsync(emailTemplate);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _dataStore.DeleteAsync(id);
    }

    public async Task<List<EmailTemplate>> GetAllAsync()
    {
        return await _dataStore.GetAllAsync();
    }

    public async Task<List<EmailTemplate>> GetAllTemplatesAsync(TriState active = TriState.Any, string? searchTerm = "")
    {
        return await _dataStore.GetAllTemplatesAsync(active, searchTerm);
    }
}