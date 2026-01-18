using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

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

    public async Task<EmailTemplate?> GetAsync(string location)
    {
        return await _dataStore.GetAsync(location);
    }

    public async Task<EmailTemplate> SaveAsync(EmailTemplate emailTemplate)
    {
        return await _dataStore.SaveAsync(emailTemplate);
    }

    public async Task<bool> DeleteAsync(string location)
    {
        return await _dataStore.DeleteAsync(location);
    }

    public async Task<List<EmailTemplate>> GetAllAsync()
    {
        return await _dataStore.GetAllAsync();
    }
}