using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Constants;
using MoreSpeakers.Domain.Models.Messages;
using MoreSpeakers.Functions.Interfaces;

namespace MoreSpeakers.Functions;

/// <summary>
/// Handles the email delivery reports
/// </summary>
/// <remarks>This is required from Azure Communication Services email in order to get and maintain a high sender rating</remarks>
public class ProcessEmailDeliveryReports
{
    private readonly ISettings _settings;
    private readonly ILogger<ProcessEmailDeliveryReports> _logger;
    private readonly TelemetryClient _telemetryClient;
    private readonly string[] _emailErrorStatuses;
    
    public ProcessEmailDeliveryReports(ISettings settings, TelemetryConfiguration telemetryConfiguration, ILogger<ProcessEmailDeliveryReports> logger)
    {
        _settings = settings;
        _telemetryClient = new TelemetryClient(telemetryConfiguration);
        _logger = logger;
        _emailErrorStatuses = settings.BouncedEmailStatuses.Split(';');
    }

    [Function(nameof(ProcessEmailDeliveryReports))]
    public async Task Run([QueueTrigger(Queues.BouncedEmails)] string messageText)
    {
        // Need to deserialize the message.  It's using base64 encoding and the other functions are not.
        var utf8Bytes = Convert.FromBase64String(messageText);
        var messageString = Encoding.UTF8.GetString(utf8Bytes);
        var message = System.Text.Json.JsonSerializer.Deserialize<EmailDeliveryReportReceived>(messageString);
        
        if (message is null)
        {
            _logger.LogError("ProcessEmailDeliveryReports: Failed to deserialize message: {Message}", messageText);
            _telemetryClient.TrackEvent("EmailDeliveryReportReceived-DeserializeFailure", new Dictionary<string, string>
            {
                {"Message", messageText}
            });
            return;
        }
        
        _logger.LogDebug("ProcessEmailDeliveryReports: Processing message for Subject \'{Subject}\', Status \'{Status}\'", message.Subject, message.Data.Status);
        
        switch (message.Data.Status)
        {
            case "Delivered":
                await HandleDeliveredEmail(message);
                return;
            case "Suppressed":
            case "Bounced":
                await HandleBouncedEmail(message);
                return;
            case "Quarantined":
            case "FilteredSpam":
                await HandleQuarantinedEmail(message);
                return;
            case "Failed":
                await HandleFailedEmail(message);
                return;
            case "Expanded":
                // No need to do anything with this
                return;
        }
    }
    
    private async Task HandleDeliveredEmail(EmailDeliveryReportReceived message)
    {
        if (string.IsNullOrEmpty(message.Data.DeliveryStatusDetails.StatusMessage))
        {
            // This is a successful send.  Nothing we need to do
            return;
        }
        
        // So far this has only happened with sending to Gmail because of the lack of DMARC on this domain.
        // That has been fixed, so I can attempt it again
        if (DoesDeliveryStatusMessageContain(message.Data.DeliveryStatusDetails.StatusMessage, _emailErrorStatuses))
        {
            _logger.LogInformation("ProcessEmailDeliveryReports: Failed (Unsubscribing User): Subject: \'{Subject}\', Recipient: \'{Recipient}\'", message.Subject, message.Data.Recipient);
            _telemetryClient.TrackEvent("EmailDeliveryReportReceived-FailedUnsubscribingUser", new Dictionary<string, string>
            {
                {"Subject", message.Subject},
                {"Recipient", message.Data.Recipient},
                {"Status", message.Data.Status},
                {"StatusMessage", message.Data.DeliveryStatusDetails.StatusMessage}
            });
            
            // TODO: Need to unsubscribe the user
            // await _userService.UnsubscribeUserAsync(message.Data.Recipient);
            return;
        }
        
        _logger.LogError("ProcessEmailDeliveryReports: Delivered with Exception: Subject: \'{Subject}\', Recipient: \'{Recipient}\'", message.Subject, message.Data.Recipient);
        _telemetryClient.TrackEvent("EmailDeliveryReportReceived-Failure", new Dictionary<string, string>
        {
            {"Subject", message.Subject},
            {"Recipient", message.Data.Recipient},
            {"Status", message.Data.Status},
            {"StatusMessage", message.Data.DeliveryStatusDetails.StatusMessage}
        });
    }
    
    private async Task HandleBouncedEmail(EmailDeliveryReportReceived message)
    {
        _logger.LogInformation("ProcessEmailDeliveryReports: Bounced: Subject: \'{Subject}\', Recipient: \'{Recipient}\'", message.Subject, message.Data.Recipient);
        _telemetryClient.TrackEvent("EmailDeliveryReportReceived-Bounced", new Dictionary<string, string>
        {
            {"Subject", message.Subject},
            {"Recipient", message.Data.Recipient},
            {"Status", message.Data.Status},
            {"StatusMessage", message.Data.DeliveryStatusDetails.StatusMessage}
        });
        
        // TODO: Need to unsubscribe the user, or mark the email address as bounced
        // await _userService.UnsubscribeUserAsync(message.Data.Recipient);
    }

    private async Task HandleQuarantinedEmail(EmailDeliveryReportReceived message)
    {
        // This happens when the email is marked as quarantined or spam.  We need to unsubscribe the user
        _logger.LogInformation("ProcessEmailDeliveryReports: Marked as Spam: Subject: \'{Subject}\', Recipient: \'{Recipient}\'", message.Subject, message.Data.Recipient);
        _telemetryClient.TrackEvent("EmailDeliveryReportReceived-Spam", new Dictionary<string, string>
        {
            {"Subject", message.Subject},
            {"Recipient", message.Data.Recipient},
            {"Status", message.Data.Status},
            {"StatusMessage", message.Data.DeliveryStatusDetails.StatusMessage}
        });
        
        // TODO: Need to unsubscribe the user, or mark the email address as spam
        // await _userService.UnsubscribeUserAsync(message.Data.Recipient);
    }

    private async Task HandleFailedEmail(EmailDeliveryReportReceived message)
    {
        // This happens when the email is marked as 'Failed'
        
        // If the status message contains any of the error statuses, then we need to unsubscribe the user
        if (DoesDeliveryStatusMessageContain(message.Data.DeliveryStatusDetails.StatusMessage, _emailErrorStatuses))
        {
            _logger.LogInformation("ProcessEmailDeliveryReports: Failed (Unsubscribing User): Subject: \'{Subject}\', Recipient: \'{Recipient}\'", message.Subject, message.Data.Recipient);
            _telemetryClient.TrackEvent("EmailDeliveryReportReceived-FailedUnsubscribingUser", new Dictionary<string, string>
            {
                {"Subject", message.Subject},
                {"Recipient", message.Data.Recipient},
                {"Status", message.Data.Status},
                {"StatusMessage", message.Data.DeliveryStatusDetails.StatusMessage}
            });
            
            // TODO: Need to unsubscribe the user
            // await _userService.UnsubscribeUserAsync(message.Data.Recipient);
            return;
        }
        
        _logger.LogError("ProcessEmailDeliveryReports: Delivery Failed: Subject: \'{Subject}\', Recipient: \'{Recipient}\'", message.Subject, message.Data.Recipient);
        _telemetryClient.TrackEvent("EmailDeliveryReportReceived-Failed", new Dictionary<string, string>
        {
            {"Subject", message.Subject},
            {"Recipient", message.Data.Recipient},
            {"Status", message.Data.Status},
            {"StatusMessage", message.Data.DeliveryStatusDetails.StatusMessage}
        });
    }
    
    private bool DoesDeliveryStatusMessageContain(string statusMessage, IEnumerable<string> searchTerms)
    {
        if (string.IsNullOrEmpty(statusMessage))
        {
            return false;
        }
        
        foreach (var searchTerm in searchTerms)
        {
            if (statusMessage.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
}