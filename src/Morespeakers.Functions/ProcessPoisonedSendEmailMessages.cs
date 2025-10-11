using Azure.Storage.Queues;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Functions.Interfaces;
using JosephGuadagno.AzureHelpers.Storage;
using Queues = MoreSpeakers.Domain.Constants.Queues;

namespace MoreSpeakers.Functions;

/// <summary>
/// Handles emails that could not be sent
/// </summary>
public class ProcessPoisonedSendEmailMessages
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ISettings _settings;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ProcessPoisonedSendEmailMessages> _logger;

    /// <summary>
    /// Initializes a new instance of the ProcessPoisonedSendEmailMessages class with the specified queue service
    /// client, application settings, telemetry client, and logger.
    /// </summary>
    /// <param name="queueServiceClient">The QueueServiceClient instance used to interact with the message queue for processing poisoned send email
    /// messages.</param>
    /// <param name="settings">The application settings that configure behavior for processing poisoned send email messages.</param>
    /// <param name="telemetryClient">The TelemetryClient used to record telemetry data and track processing metrics.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for this instance.</param>
    /// <remarks>Handles poison emails.  Generally it just needs to try them again.</remarks>
    public ProcessPoisonedSendEmailMessages(QueueServiceClient queueServiceClient, ISettings settings, TelemetryClient telemetryClient, ILogger<ProcessPoisonedSendEmailMessages> logger)
    {
        _queueServiceClient = queueServiceClient;
        _settings = settings;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    [Function("ProcessPoisonedSendEmailMessages")]
    public async Task Run([TimerTrigger("%ProcessPoisonedSendEmailMessages_CronSettings%")] TimerInfo myTimer)
    {
        _logger.LogDebug("ProcessPoisonedSendEmailMessages: Timer trigger function executed at: {Now}", DateTime.Now);

        var poisonQueueClient = _queueServiceClient.GetQueueClient(Queues.SendEmailPoison);

        var messageCount = 0;
        if (await poisonQueueClient.ExistsAsync())
        {
            var poisonMessages = await poisonQueueClient.ReceiveMessagesAsync(30);
            if (poisonMessages is not null)
            {
                var sendEmailQueue = new Queue(_queueServiceClient, Queues.SendEmail);
                foreach (var poisonMessage in poisonMessages.Value)
                {
                    await sendEmailQueue.AddMessageAsync(poisonMessage);
                    await poisonQueueClient.DeleteMessageAsync(poisonMessage.MessageId, poisonMessage.PopReceipt);
                    messageCount++;
                    _telemetryClient.TrackEvent("PoisonedSendEmailMessageReprocessed", new Dictionary<string, string>
                    {
                        {"MessageId", poisonMessage.MessageId},
                        {"InsertionTime", poisonMessage.InsertedOn?.ToString("o") ?? string.Empty},
                        {"NextVisibleTime", poisonMessage.NextVisibleOn?.ToString("o") ?? string.Empty},
                        {"DequeueCount", poisonMessage.DequeueCount.ToString()}
                    });
                }
            }
        }
        
        _logger.LogDebug("ProcessPoisonedSendEmailMessages: Timer trigger function completed at: {Now}, {MessageCount} messages processed", DateTime.Now, messageCount);
    }
}