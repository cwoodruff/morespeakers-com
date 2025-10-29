using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;

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
}