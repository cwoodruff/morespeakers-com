using Azure.Storage.Queues;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Constants;
using MoreSpeakers.Functions.Interfaces;

public class ProcessPoisonedSendEmailMessages
{
    private readonly ISettings _settings;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ProcessPoisonedSendEmailMessages> _logger;

    public ProcessPoisonedSendEmailMessages(ISettings settings, TelemetryClient telemetryClient, ILogger<ProcessPoisonedSendEmailMessages> logger)
    {
        _settings = settings;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    [Function("ProcessPoisonedSendEmailMessages")]
    public async Task Run([TimerTrigger("0 */20 * * * *")] TimerInfo myTimer)
    {
        _logger.LogDebug("ProcessPoisonedSendEmailMessages: Timer trigger function executed at: {Now}", DateTime.Now);

        var poisonQueueClient = new QueueClient(_settings.AzureStorageConnectionString, Queues.SendEmailPoison);

        var messageCount = 0;
        if (await poisonQueueClient.ExistsAsync())
        {
            var poisonMessages = await poisonQueueClient.ReceiveMessagesAsync(30);
            if (poisonMessages is not null)
            {
                var sendEmailsQueueClient = new QueueClient(_settings.AzureStorageConnectionString, Queues.SendEmail);
                foreach (var poisonMessage in poisonMessages.Value)
                {
                    await sendEmailsQueueClient.SendMessageAsync(poisonMessage.MessageText);
                    await poisonQueueClient.DeleteMessageAsync(poisonMessage.MessageId, poisonMessage.PopReceipt);
                    messageCount++;
                }
            }
        }
        
        _logger.LogDebug("ProcessPoisonedSendEmailMessages: Timer trigger function completed at: {Now}, {MessageCount} messages processed", DateTime.Now, messageCount);
    }
}