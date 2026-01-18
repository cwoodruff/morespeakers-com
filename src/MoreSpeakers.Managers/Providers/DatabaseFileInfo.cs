using Microsoft.Extensions.FileProviders;
using MoreSpeakers.Domain.Models;
using System.Text;

namespace MoreSpeakers.Managers.Providers;

public class DatabaseFileInfo : IFileInfo
{
    private readonly EmailTemplate? _template;
    private readonly string _name;

    public DatabaseFileInfo(EmailTemplate? template, string name)
    {
        _template = template;
        _name = name;
    }

    public bool Exists => _template != null;
    public long Length => _template != null ? Encoding.UTF8.GetByteCount(_template.Content) : -1;
    public string? PhysicalPath => null;
    public string Name => _name;
    public DateTimeOffset LastModified => _template?.LastModified ?? DateTimeOffset.MinValue;
    public bool IsDirectory => false;

    public Stream CreateReadStream()
    {
        if (_template == null)
        {
            throw new FileNotFoundException();
        }

        return new MemoryStream(Encoding.UTF8.GetBytes(_template.Content));
    }
}