using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface IEmailTemplateManager
{
    Task<EmailTemplate?> GetAsync(int id);
    Task<EmailTemplate?> GetByLocationAsync(string location);
    Task<EmailTemplate> SaveAsync(EmailTemplate emailTemplate);
    Task<bool> DeleteAsync(int id);
    Task<List<EmailTemplate>> GetAllAsync();
    Task<List<EmailTemplate>> GetAllTemplatesAsync(TriState active = TriState.Any, string? searchTerm = "");
}