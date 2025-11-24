using System.Text.Json;

using MoreSpeakers.Domain.Models.DTOs;
using MoreSpeakers.Domain.Models.Messages;

namespace MoreSpeakers.Domain.Tests;

public class DtoSerializationTests
{
    [Fact]
    public void GitHubContributorSerializesSuccessfully()
    {
        var dto = new GitHubContributor
        {
            Login = "octocat",
            AvatarUrl = "https://localhost/calamari",
            HtmlUrl = "https://localhost/calico",
            Contributions = 42
        };

        var json = JsonSerializer.Serialize(dto);
        Assert.Contains("\"login\":\"octocat\"", json);
        Assert.Contains("\"avatar_url\":\"https://localhost/calamari\"", json);
        Assert.Contains("\"html_url\":\"https://localhost/calico\"", json);
        Assert.Contains("\"contributions\":42", json);
    }
}

public class MessageSerializationTests
{
    [Fact]
    public void MessageDtoSerializesSuccessfully()
    {
        var message = new Data
        {
            Sender = "Sender",
            Recipient = "Recipient",
            MessageId = "MessageId",
            Status = "Status",
            DeliveryStatusDetails = new DeliveryStatusDetails
            {
                StatusMessage = "StatusMessage"
            },
            DeliveryAttemptTimeStamp = new DateTime(new DateOnly(2025, 11, 5), default, DateTimeKind.Utc)
        };
        var json = JsonSerializer.Serialize(message);
        Assert.Contains("\"sender\":\"Sender\"", json);
        Assert.Contains("\"recipient\":\"Recipient\"", json);
        Assert.Contains("\"messageId\":\"MessageId\"", json);
        Assert.Contains("\"status\":\"Status\"", json);
        Assert.Contains("\"deliveryStatusDetails\":{\"statusMessage\":\"StatusMessage\"}", json);
        Assert.Contains("\"deliveryAttemptTimeStamp\":\"2025-11-05T00:00:00Z\"", json);
    }

    [Fact]
    public void EmailMessageBaseSerializesSuccessfully()
    {
        var message = new EmailMessageBase
        {
            Id = Guid.Parse("12345678-1234-1234-1234-1234567890ab"),
            Topic = "Topic",
            Subject = "Subject",
            EventType = "EventType",
            DataVersion = "1.0",
            MetadataVersion = "1.0",
            EventTime = new DateTime(new DateOnly(2025, 11, 5), default, DateTimeKind.Utc)
        };
        var json = JsonSerializer.Serialize(message);
        Assert.Contains("\"id\":\"12345678-1234-1234-1234-1234567890ab\"", json);
        Assert.Contains("\"topic\":\"Topic\"", json);
        Assert.Contains("\"subject\":\"Subject\"", json);
        Assert.Contains("\"eventType\":\"EventType\"", json);
        Assert.Contains("\"dataVersion\":\"1.0\"", json);
        Assert.Contains("\"metadataVersion\":\"1.0\"", json);
        Assert.Contains("\"eventTime\":\"2025-11-05T00:00:00Z\"", json);
    }

    [Fact]
    public void EmailEngagementTrackingReportReceivedDataSerializesSuccessfully()
    {
        var message = new EmailEngagementTrackingReportReceivedData
        {
            Sender = "Sender",
            MessageId = Guid.Parse("12345678-1234-1234-1234-1234567890ab"),
            UserActionTimeStamp = new DateTime(new DateOnly(2025, 11, 5), default, DateTimeKind.Utc),
            EngagementContext = "EngagementContext",
            UserAgent = "UserAgent",
            EngagementType = "View"
        };
        var json = JsonSerializer.Serialize(message);
        Assert.Contains("\"sender\":\"Sender\"", json);
        Assert.Contains("\"messageId\":\"12345678-1234-1234-1234-1234567890ab\"", json);
        Assert.Contains("\"userActionTimeStamp\":\"2025-11-05T00:00:00Z\"", json);
        Assert.Contains("\"engagementContext\":\"EngagementContext\"", json);
        Assert.Contains("\"userAgent\":\"UserAgent\"", json);
        Assert.Contains("\"engagementType\":\"View\"", json);
    }

    [Fact]
    public void EmailDeliveryReportReceivedDataSerializesSuccessfully()
    {
        var message = new EmailDeliveryReportReceivedData
        {
            Sender = "Sender",
            Recipient = "Recipient",
            MessageId = Guid.Parse("12345678-1234-1234-1234-1234567890ab"),
            Status = "Status",
            DeliveryStatusDetails = new EmailDeliveryReportReceivedDeliveryStatusDetails
            {
                StatusMessage = "StatusMessage"
            },
            DeliveryAttemptTimestamp = new DateTime(new DateOnly(2025, 11, 5), default, DateTimeKind.Utc)
        };
        var json = JsonSerializer.Serialize(message);
        Assert.Contains("\"sender\":\"Sender\"", json);
        Assert.Contains("\"recipient\":\"Recipient\"", json);
        Assert.Contains("\"messageId\":\"12345678-1234-1234-1234-1234567890ab\"", json);
        Assert.Contains("\"status\":\"Status\"", json);
        Assert.Contains("\"deliveryStatusDetails\":{\"statusMessage\":\"StatusMessage\"}", json);
        Assert.Contains("\"deliveryAttemptTimestamp\":\"2025-11-05T00:00:00Z\"", json);
    }

    [Fact]
    public void EmailEngagementTrackingReportReceivedSerializesSuccessfully()
    {
        var messageId = Guid.NewGuid();
        var dataMessageId = Guid.NewGuid();
        var message = new EmailEngagementTrackingReportReceived
        {
            Id = messageId,
            Topic = "Topic",
            Subject = "Subject",
            Data = new EmailEngagementTrackingReportReceivedData
            {
                Sender = "Sender",
                MessageId = dataMessageId,
                UserActionTimeStamp = new DateTime(new DateOnly(2025, 11, 5), default, DateTimeKind.Utc),
                EngagementContext = "EngagementContext",
                UserAgent = "UserAgent",
                EngagementType = "View"
            },
            EventType = "EventType",
            DataVersion = "1.0",
            MetadataVersion = "1.0",
            EventTime = new DateTime(new DateOnly(2025, 11, 5), default, DateTimeKind.Utc)
        };
        var json = JsonSerializer.Serialize(message);
        Assert.Contains($"\"id\":\"{messageId}\"", json);
        Assert.Contains("\"topic\":\"Topic\"", json);
        Assert.Contains("\"subject\":\"Subject\"", json);
        Assert.Contains($"\"data\":{{\"sender\":\"Sender\",\"messageId\":\"{dataMessageId}\",\"userActionTimeStamp\":\"2025-11-05T00:00:00Z\",\"engagementContext\":\"EngagementContext\",\"userAgent\":\"UserAgent\",\"engagementType\":\"View\"}}", json);
        Assert.Contains("\"eventType\":\"EventType\"", json);
        Assert.Contains("\"dataVersion\":\"1.0\"", json);
        Assert.Contains("\"metadataVersion\":\"1.0\"", json);
        Assert.Contains("\"eventTime\":\"2025-11-05T00:00:00Z\"", json);
    }

    [Fact]
    public void EmailDeliveryReportReceivedDeliveryStatusDetailsSerializesSuccessfully()
    {
        var details = new EmailDeliveryReportReceivedDeliveryStatusDetails
        {
            StatusMessage = "StatusMessage"
        };
        var json = JsonSerializer.Serialize(details);
        Assert.Contains("\"statusMessage\":\"StatusMessage\"", json);
    }
}