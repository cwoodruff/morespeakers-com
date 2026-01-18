using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IEmailTemplateManager
{
    Task<EmailTemplate?> GetAsync(string location);
    Task<EmailTemplate> SaveAsync(EmailTemplate emailTemplate);
    Task<bool> DeleteAsync(string location);
    Task<List<EmailTemplate>> GetAllAsync();
}