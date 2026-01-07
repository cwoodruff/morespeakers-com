using System.Text;

using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using JosephGuadagno.AzureHelpers.Storage;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.Messages;

using SixLabors.ImageSharp;

using ISettings = MoreSpeakers.Functions.Interfaces.ISettings;
using Queues = MoreSpeakers.Domain.Constants.Queues;

namespace MoreSpeakers.Functions;

public class ProcessOpenGraphSpeakerProfileImageGenerationMessage
{
    private readonly IOpenGraphSpeakerProfileImageGenerator _openGraphSpeakerProfileImageGenerator;
    private readonly QueueServiceClient _queueServiceClient;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ISettings _settings;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ProcessOpenGraphSpeakerProfileImageGenerationMessage> _logger;

    public ProcessOpenGraphSpeakerProfileImageGenerationMessage(
        IOpenGraphSpeakerProfileImageGenerator openGraphSpeakerProfileImageGenerator,
        BlobServiceClient blobServiceClient,
        QueueServiceClient queueServiceClient,
        ISettings settings,
        TelemetryClient telemetryClient,
        ILogger<ProcessOpenGraphSpeakerProfileImageGenerationMessage> logger)
    {
        _openGraphSpeakerProfileImageGenerator = openGraphSpeakerProfileImageGenerator;
        _blobServiceClient = blobServiceClient;
        _queueServiceClient = queueServiceClient;
        _settings = settings;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    [Function("ProcessOpenGraphSpeakerProfileImageGenerationMessage")]
    public async Task Run([QueueTrigger(Queues.CreateOpenGraphProfileImage)] CreateOpenGraphProfileImage createOpenGraphProfileImage)
    {
        _logger.LogDebug("ProcessOpenGraphSpeakerProfileImageGenerationMessage: Generating OpenGraph profile image card for UserId \'{UserId}\'", createOpenGraphProfileImage.UserId);

        try
        {
            // Create the image
            var speakerImage = await _openGraphSpeakerProfileImageGenerator.GenerateSpeakerProfileFromUrlsAsync(createOpenGraphProfileImage.ProfileImageUrl, _settings.LogoImageUrl, createOpenGraphProfileImage.SpeakerName);

            // Save the image to blob storage
            var blobName = $"{createOpenGraphProfileImage.UserId}.png";
            var blobContainer = _blobServiceClient.GetBlobContainerClient(Domain.Constants.Blobs.OpenGroupProfileImage);
            await blobContainer.CreateIfNotExistsAsync();
            var blobs = new Blobs(blobContainer);
            var speakerImageStream = new MemoryStream();
            await speakerImage.SaveAsPngAsync(speakerImageStream);
            speakerImageStream.Position = 0;
            await blobs.UploadAndOverwriteIfExistsAsync(blobName, speakerImageStream);

            // If we made it here, we're done
            _telemetryClient.TrackEvent(Domain.Constants.TelemetryEvents.OpenGraph.OpenGraphProfileImageCreated,
                new Dictionary<string, string> { { "UserId", createOpenGraphProfileImage.UserId.ToString() } });
            _logger.LogDebug("ProcessOpenGraphSpeakerProfileImageGenerationMessage: Done generating OpenGraph profile image card for UserId \'{UserId}\'", createOpenGraphProfileImage.UserId);
        }
        catch
        {
            _logger.LogError("ProcessOpenGraphSpeakerProfileImageGenerationMessage: Failed to generate OpenGraph profile image card for UserId '{UserId}', url: '{Url}", createOpenGraphProfileImage.UserId, createOpenGraphProfileImage.ProfileImageUrl);
            throw;
        }
    }
}