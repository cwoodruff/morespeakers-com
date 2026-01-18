using Microsoft.Extensions.Primitives;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers.Providers;

public class DatabaseChangeToken : IChangeToken
{
    private readonly EmailTemplate _emailTemplate;

    public DatabaseChangeToken(EmailTemplate emailTemplate)
    {
        _emailTemplate = emailTemplate;
    }

    public bool HasChanged
    {
        get
        {
            if (_emailTemplate.LastRequested.HasValue)
            {
                return _emailTemplate.LastModified > _emailTemplate.LastRequested.Value;
            }

            return false;
        }
    }

    public bool ActiveChangeCallbacks => false;

    public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NullDisposable.Instance;

    private class NullDisposable : IDisposable
    {
        public static NullDisposable Instance { get; } = new();
        public void Dispose() { }
    }
}