namespace MoreSpeakers.Domain.Constants;

/// <summary>
/// Names of telemetry events that are sent to Application Insights.
/// </summary>
public static class TelemetryEvents
{
    public const string WelcomeEmail = "WelcomeEmail";
    public const string EmailConfirmation = "EmailConfirmation";
    public const string MentorshipRequested = "MentorshipRequested";
    public const string MentorshipCancelled = "MentorshipCancelled";
    public const string MentorshipDeclined = "MentorshipDeclined";
    public const string MentorshipAccepted = "MentorshipAccepted";
}