using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface IEmailTemplateDataStore : IDataStorePrimaryKeyInt<EmailTemplate>
{
    Task<EmailTemplate?> GetByLocationAsync(string location);
    Task<List<EmailTemplate>> GetAllTemplatesAsync(TriState active = TriState.Any, string? searchTerm = "");
}