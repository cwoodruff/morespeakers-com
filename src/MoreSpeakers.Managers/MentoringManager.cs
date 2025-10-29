using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class MentoringManager: IMentoringManager
{
    private readonly IMentoringDataStore _dataStore;
    private readonly ILogger<MentoringManager> _logger;

    public MentoringManager(IMentoringDataStore dataStore, ILogger<MentoringManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }
    
    public async Task<Mentorship> GetAsync(Guid primaryKey)
    {
        return await _dataStore.GetAsync(primaryKey);
    }

    public async Task<bool> DeleteAsync(Guid primaryKey)
    {
        return await _dataStore.DeleteAsync(primaryKey);
    }

    public async Task<Mentorship> SaveAsync(Mentorship entity)
    {
        return await _dataStore.SaveAsync(entity);
    }

    public async Task<List<Mentorship>> GetAllAsync()
    {
        return await _dataStore.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(Mentorship entity)
    {
        return await _dataStore.DeleteAsync(entity);
    }

    public async Task<List<Expertise>> GetSharedExpertisesAsync(User mentor, User mentee)
    {
        return await _dataStore.GetSharedExpertisesAsync(mentor, mentee);
    }

    public async Task<bool> DoesMentorshipRequestsExistsAsync(User mentor, User mentee)
    {
        return await _dataStore.DoesMentorshipRequestsExistsAsync(mentor, mentee);
    }

    public async Task<bool> CreeateMentorshipRequestAsync(Mentorship mentorship)
    {
        return await _dataStore.CreeateMentorshipRequestAsync(mentorship);
    }
}