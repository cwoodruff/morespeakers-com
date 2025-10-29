using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IMentoringDataStore: IDataStorePrimaryKeyGuid<Mentorship>
{
    public Task<List<Expertise>> GetSharedExpertisesAsync(User mentor, User mentee);
    public Task<bool> DoesMentorshipRequestsExistsAsync(User mentor, User mentee);
    public Task<bool> CreeateMentorshipRequestAsync(Mentorship mentorship);
}