using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Services;

public interface IOpenGraphService
{
    Dictionary<string, string> GenerateUserMetadata(User user, string profileUrl);
}