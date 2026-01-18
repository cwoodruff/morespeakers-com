using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Managers.Providers;

public class DatabaseFileProvider : IFileProvider
{
    private readonly IEmailTemplateManager _emailTemplateManager;

    public DatabaseFileProvider(IEmailTemplateManager emailTemplateManager)
    {
        _emailTemplateManager = emailTemplateManager;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (string.IsNullOrEmpty(subpath))
        {
            return new NotFoundFileInfo(subpath);
        }

        var template = _emailTemplateManager.GetAsync(subpath).GetAwaiter().GetResult();
        return template != null ? new DatabaseFileInfo(template, subpath) : new NotFoundFileInfo(subpath);
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return NotFoundDirectoryContents.Singleton;
    }

    public IChangeToken Watch(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return NullChangeToken.Singleton;
        }

        var template = _emailTemplateManager.GetAsync(filter).GetAwaiter().GetResult();
        return template != null ? new DatabaseChangeToken(template) : NullChangeToken.Singleton;
    }
}