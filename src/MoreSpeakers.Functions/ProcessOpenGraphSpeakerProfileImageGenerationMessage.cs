using Azure.Storage.Blobs;
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
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ISettings _settings;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ProcessOpenGraphSpeakerProfileImageGenerationMessage> _logger;

    public ProcessOpenGraphSpeakerProfileImageGenerationMessage(
        IOpenGraphSpeakerProfileImageGenerator openGraphSpeakerProfileImageGenerator,
        BlobServiceClient blobServiceClient,
        ISettings settings,
        TelemetryClient telemetryClient,
        ILogger<ProcessOpenGraphSpeakerProfileImageGenerationMessage> logger)
    {
        _openGraphSpeakerProfileImageGenerator = openGraphSpeakerProfileImageGenerator;
        _blobServiceClient = blobServiceClient;
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
            // Load the Ubuntu font
            var fileName = "Ubuntu-R.ttf";
            var fontDirectory = FindFileInCurrentDirectoryOrParentDirectories(fileName);
            if (string.IsNullOrEmpty(fontDirectory))
            {
                _logger.LogError("ProcessOpenGraphSpeakerProfileImageGenerationMessage: Unable to find font '{FileName}'", fileName);
                throw new ApplicationException($"Unable to find font file '{fileName}'");
            }
            var fontFile = Path.Combine(fontDirectory, fileName);
            var ubuntuFont = _openGraphSpeakerProfileImageGenerator.GetFontFamilyFromFile(fontFile);
            if (ubuntuFont == null)
            {
                _logger.LogError("ProcessOpenGraphSpeakerProfileImageGenerationMessage: Unable to load font '{FileName}'", fontFile);
                throw new ApplicationException($"Unable to load font '{fontFile}'");
            }

            // Create the image
            var speakerImage = await _openGraphSpeakerProfileImageGenerator.GenerateSpeakerProfileFromUrlsAsync(
                createOpenGraphProfileImage.ProfileImageUrl, _settings.LogoImageUrl,
                createOpenGraphProfileImage.SpeakerName, ubuntuFont.Value);

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
        catch (Exception e)
        {
            _logger.LogError(e,"ProcessOpenGraphSpeakerProfileImageGenerationMessage: Failed to generate OpenGraph profile image card for UserId '{UserId}', url: '{Url}", createOpenGraphProfileImage.UserId, createOpenGraphProfileImage.ProfileImageUrl);
            throw;
        }
    }

    private string FindFileInCurrentDirectoryOrParentDirectories(string fileName)
    {
        var currentDirectory = Environment.CurrentDirectory;
        var root = Path.GetPathRoot(currentDirectory);

        while (currentDirectory != root && currentDirectory != string.Empty && Path.Exists(currentDirectory))
        {

            // Check if any files in the current directory match the wildcards
            if (Directory.GetFiles(currentDirectory, fileName).Any())
            {
                return currentDirectory;
            }

            // Move to the parent directory
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }

        return string.Empty;
    }
}