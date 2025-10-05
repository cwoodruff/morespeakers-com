using Azure;
using Azure.Communication.Email;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Constants;
using MoreSpeakers.Domain.Models.Messages;
using MoreSpeakers.Functions.Interfaces;

public class SendEmail
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<SendEmail> _logger;
    private readonly ISettings _settings;

    public SendEmail(ISettings settings, TelemetryConfiguration telemetryConfiguration, ILogger<SendEmail> logger)
    {
        _telemetryClient = new TelemetryClient(telemetryConfiguration);
        _logger = logger;
        _settings = settings;
    }
    
    [Function(nameof(SendEmail))]
    public async Task RunAsync([QueueTrigger(Queues.SendEmail)] Email emailMessage)
    {
        // Note: For this to work with Azure Communication Services, the emailMessage.FromMailAddress 
        // must be a valid email address that is registered in the Azure Communication Services portal.
        // Currently, only 'DoNotReply@desertcodecamp.com' is registered.
        
        _logger.LogDebug("SendEmail: Processing message for Subject \'{Subject}\'", emailMessage.Subject);

        var emailClient = new EmailClient(_settings.AzureCommunicationsConnectionString);

        var emailContent = new EmailContent(emailMessage.Subject)
        {
            PlainText = emailMessage.Body,
            Html = $"<html><body>{emailMessage.Body}</body></html>"
        };

        var emailAddresses = new List<EmailAddress>
        {
            new(emailMessage.ToMailAddress, emailMessage.ToDisplayName)
        };
        var emailRecipients = new EmailRecipients(emailAddresses);
        var email = new EmailMessage(emailMessage.FromMailAddress, emailRecipients, emailContent);
        
        try
        {
            var emailResult = await emailClient.SendAsync(WaitUntil.Started, email);
            _telemetryClient.TrackEvent("EmailSent");
            if (emailResult is null)
            {
                _logger.LogError("SendEmail: Failed to send email for Subject \'{Subject}\' for \'{EmailAddress}'", emailMessage.Subject, emailMessage.ToMailAddress);
            }
            else
            {
                _logger.LogDebug("SendEmail: Successfully sent email for Subject \'{Subject}\'", emailMessage.Subject);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("SendEmail: Failed to send email for Subject \'{Subject}\' for \'{EmailAddress}', Exception: {Message}", emailMessage.Subject, emailMessage.ToMailAddress, e.Message);
            throw;
        }
    }
}