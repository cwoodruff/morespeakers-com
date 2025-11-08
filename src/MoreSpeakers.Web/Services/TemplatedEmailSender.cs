using Microsoft.ApplicationInsights;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Services;

public class TemplatedEmailSender: ITemplatedEmailSender
{
    private readonly IEmailSender _emailSender;
    private readonly IRazorPartialToStringRenderer _stringRenderer;
    private readonly ILogger<TemplatedEmailSender> _logger;
    private readonly TelemetryClient _telemetryClient;

    /// <summary>
    /// Sends an email based on a Razor template
    /// </summary>
    /// <param name="emailSender">A class to send emails</param>
    /// <param name="stringRenderer">The renderer to use to build the body for the email</param>
    /// <param name="logger">The logger</param>
    /// <param name="telemetryClient">The telemetry client</param>
    public TemplatedEmailSender(IEmailSender emailSender, IRazorPartialToStringRenderer stringRenderer,
        ILogger<TemplatedEmailSender> logger, TelemetryClient telemetryClient)
    {
        _emailSender = emailSender;
        _stringRenderer = stringRenderer;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }
    
    public async Task<bool> SendTemplatedEmail(string emailTemplate, string telemetryEventName, string subject, User toUser, object? model)
    {
        if (string.IsNullOrWhiteSpace(emailTemplate))
        {
            throw new ArgumentException("Email template cannot be null or whitespace", nameof(emailTemplate));
        }
        if (string.IsNullOrWhiteSpace(telemetryEventName))
        {
            throw new ArgumentException("Event name cannot be null or whitespace", nameof(telemetryEventName));
        }
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Email subject cannot be null or whitespace", nameof(subject));
        }

        try
        {
            var emailBody = await _stringRenderer.RenderPartialToStringAsync(emailTemplate, model);
            await _emailSender.QueueEmail(new System.Net.Mail.MailAddress(toUser.Email!, $"{toUser.FirstName} {toUser.LastName}"),
                subject, emailBody);

            _telemetryClient.TrackEvent(telemetryEventName, new Dictionary<string, string>
            {
                { "UserId", toUser.Id.ToString() },
                { "Email", toUser.Email! }
            });
            _logger.LogInformation("{EventName} email was successfully sent to {Email}", telemetryEventName, toUser.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {EventName} email to {Email}", telemetryEventName, toUser.Email);
            return false;
        }

        return true;
    }
}