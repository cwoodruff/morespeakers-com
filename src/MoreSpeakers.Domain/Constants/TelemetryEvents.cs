namespace MoreSpeakers.Domain.Constants;

/// <summary>
/// Names of telemetry events that are sent to Application Insights.
/// </summary>
public static class TelemetryEvents
{
    public static class EmailGenerated
    {
        public const string Welcome = "Email-Welcome";
        public const string Confirmation = "Email-Confirmation";
        public const string MentorshipRequested = "Email-MentorshipRequested";
        public const string MentorshipCancelled = "Email-MentorshipCancelled";
        public const string MentorshipDeclined = "Email-MentorshipDeclined";
        public const string MentorshipAccepted = "Email-MentorshipAccepted";
    }

    public static class ManagerEvents
    {
        public const string MentorshipRequested = "Manager-MentorshipRequested";
        public const string MentorshipRequestedWithDetails = "Manager-MentorshipRequestedWithDetails";
        public const string MentorshipCancelled = "Manager-MentorshipCancelled";
        public const string MentorshipDeclined = "Manager-MentorshipDeclined";
        public const string MentorshipAccepted = "Manager-MentorshipAccepted";
        public const string MentorshipCompleted = "Manager-MentorshipCompleted";
    }
}