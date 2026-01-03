using System.Text.Json;

using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.Messages;
using JosephGuadagno.AzureHelpers.Storage;

namespace MoreSpeakers.Managers;

public class OpenGraphGenerator: IOpenGraphGenerator
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ISettings _settings;
    private readonly ILogger<OpenGraphGenerator> _logger;

    public OpenGraphGenerator(QueueServiceClient queueServiceClient, ISettings settings, ILogger<OpenGraphGenerator> logger)
    {
        _queueServiceClient = queueServiceClient;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Queues a message to create an OpenGraph profile image for a speaker.
    /// </summary>
    /// <param name="id">The unique identifier of the speaker.</param>
    /// <param name="headshotUrl">The URL of the speaker's headshot image.</param>
    public async Task QueueSpeakerOpenGraphProfileImageCreation(Guid id, string? headshotUrl = null)
    {
        var message = new CreateOpenGraphProfileImage { UserId = id, ProfileImageUrl = headshotUrl };
        var queue = new Queue(_queueServiceClient, Domain.Constants.Queues.CreateOpenGraphProfileImage);
        await queue.AddMessageAsync(JsonSerializer.Serialize(message));
        _logger.LogInformation("Queued OpenGraph profile image creation for speaker {Id}", id);
    }


}