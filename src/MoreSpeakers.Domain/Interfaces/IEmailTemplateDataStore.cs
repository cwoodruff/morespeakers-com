using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IEmailTemplateDataStore : IDataStorePrimaryKeyString<EmailTemplate>
{
    Task<EmailTemplate?> GetByLocationAsync(string location);
}