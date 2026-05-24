using FluentAssertions;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Services;
using MoreSpeakers.Web.Tests.Utility;

namespace MoreSpeakers.Web.Tests;

public sealed class TemplatedEmailSenderTests : IDisposable
{
    readonly FakeLogger<TemplatedEmailSender> _fakeLogger = new();
    readonly FakeTelemetryChannel _fakeTelemetryChannel = new();
    readonly Mock<IRazorPartialToStringRenderer> _stringRendererMock = new ();

    readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        UserName = "Franklin W. Dixon",
        Email = "leslie@mcfarlane.com",
        NormalizedEmail = "leslie@mcfarlane.com",
        EmailConfirmed = true,
    };

    readonly TemplatedEmailSender _templatedEmailSender;

    public TemplatedEmailSenderTests()
    {
        _templatedEmailSender = new TemplatedEmailSender(Mock.Of<IEmailSender>(),
            _stringRendererMock.Object,
            _fakeLogger,
            CreateStubTelemetryClient(_fakeTelemetryChannel));
    }

    [Fact]
    public async Task SendTemplatedEmail_LogsSuccess()
    {
        var result = await _templatedEmailSender.SendTemplatedEmail("template", "eventName", "subject", _user, null);
        Assert.True(result.IsSuccess);
        var fakeLogRecords = _fakeLogger.Collector.GetSnapshot();
        Assert.Single(fakeLogRecords, e=>e.Level == LogLevel.Information && e.Message == $"eventName email was successfully sent to {_user.Email}");
    }

    [Fact]
    public async Task SendTemplatedEmail_LogsException()
    {
        _stringRendererMock.Setup(mock => mock.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new InvalidOperationException("Rendering failed"));
        var result = await _templatedEmailSender.SendTemplatedEmail("template", "eventName", "subject", _user, null);
        Assert.True(result.IsFailure);
        Assert.Equal("email.render-failed", result.Error.Code);
        var fakeLogRecords = _fakeLogger.Collector.GetSnapshot();
        Assert.Single(fakeLogRecords, e=>e.Level == LogLevel.Error && e.Message == $"Failed to send eventName email to {_user.Email}");
    }

    [Fact]
    public async Task SendTemplatedEmail_EmitsSuccessTelemetry()
    {
        var result = await _templatedEmailSender.SendTemplatedEmail("template", "eventName", "subject", _user, null);
        Assert.True(result.IsSuccess);
        var telemetryEntry = Assert.Single(_fakeTelemetryChannel.SentTelemetries);
        var supportProperties = Assert.IsType<ISupportProperties>(telemetryEntry, exactMatch: false);

        Assert.True(supportProperties.Properties.ContainsKey("Email"));
        Assert.Equal(_user.Email, supportProperties.Properties["Email"]);
        Assert.True(supportProperties.Properties.ContainsKey("UserId"));
        Assert.Equal(_user.Id.ToString(), supportProperties.Properties["UserId"]);
    }

    [Fact]
    public async Task SendTemplatedEmail_ThrowsOnMissingEmailTemplate()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _templatedEmailSender.SendTemplatedEmail(
            emailTemplate: string.Empty,
            telemetryEventName: string.Empty,
            subject: string.Empty,
            toUser: new User(),
            model: null));
        Assert.Equal("emailTemplate", ex.ParamName);
    }

    [Fact]
    public async Task SendTemplatedEmail_ThrowsOnMissingTelemetryEventName()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _templatedEmailSender.SendTemplatedEmail(
            emailTemplate: "emailTemplate",
            telemetryEventName: string.Empty,
            subject: string.Empty,
            toUser: new User(),
            model: null));
        Assert.Equal("telemetryEventName", ex.ParamName);
    }


    [Fact]
    public async Task SendTemplatedEmail_ThrowsOnMissingSubject()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _templatedEmailSender.SendTemplatedEmail(
            emailTemplate: "emailTemplate",
            telemetryEventName: "telemetryEventName",
            subject: string.Empty,
            toUser: new User(),
            model: null));
        Assert.Equal("subject", ex.ParamName);
    }

    private static TelemetryClient CreateStubTelemetryClient(ITelemetryChannel telemetryChannel)
    {
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = telemetryChannel,
            DisableTelemetry = false
        };
        return new TelemetryClient(telemetryConfiguration);
    }

    public void Dispose()
    {
        _fakeTelemetryChannel.Dispose();
    }

    // ───────────────────────────────────────────────────────────────────────────
    // Whitespace guard supplemental tests
    // ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendTemplatedEmail_ShouldThrowArgumentException_WhenEmailTemplateIsWhitespace()
    {
        // Arrange / Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _templatedEmailSender.SendTemplatedEmail(
                emailTemplate: "   ",
                telemetryEventName: "eventName",
                subject: "subject",
                toUser: new User(),
                model: null));

        // Assert
        ex.ParamName.Should().Be("emailTemplate");
    }

    [Fact]
    public async Task SendTemplatedEmail_ShouldThrowArgumentException_WhenTelemetryEventNameIsWhitespace()
    {
        // Arrange / Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _templatedEmailSender.SendTemplatedEmail(
                emailTemplate: "template",
                telemetryEventName: "\t",
                subject: "subject",
                toUser: new User(),
                model: null));

        // Assert
        ex.ParamName.Should().Be("telemetryEventName");
    }

    [Fact]
    public async Task SendTemplatedEmail_ShouldThrowArgumentException_WhenSubjectIsWhitespace()
    {
        // Arrange / Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _templatedEmailSender.SendTemplatedEmail(
                emailTemplate: "template",
                telemetryEventName: "eventName",
                subject: "  ",
                toUser: new User(),
                model: null));

        // Assert
        ex.ParamName.Should().Be("subject");
    }

    // ───────────────────────────────────────────────────────────────────────────
    // Email-sender integration supplemental tests
    // ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendTemplatedEmail_ShouldQueueEmailToCorrectRecipient_WhenAllArgumentsAreValid()
    {
        // Arrange – use a dedicated mock so we can verify the call
        var emailSenderMock = new Mock<IEmailSender>();
        var sut = new TemplatedEmailSender(
            emailSenderMock.Object,
            _stringRendererMock.Object,
            _fakeLogger,
            CreateStubTelemetryClient(_fakeTelemetryChannel));

        // Act
        await sut.SendTemplatedEmail("template", "eventName", "Test Subject", _user, null);

        // Assert – QueueEmail called once with the user's address and the correct subject
        emailSenderMock.Verify(e =>
            e.QueueEmail(
                It.Is<System.Net.Mail.MailAddress>(m => m.Address == _user.Email),
                "Test Subject",
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTemplatedEmail_ShouldReturnFalse_WhenEmailSenderThrows()
    {
        // Arrange
        var emailSenderMock = new Mock<IEmailSender>();
        emailSenderMock
            .Setup(e => e.QueueEmail(
                It.IsAny<System.Net.Mail.MailAddress>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP connection refused"));

        var sut = new TemplatedEmailSender(
            emailSenderMock.Object,
            _stringRendererMock.Object,
            _fakeLogger,
            CreateStubTelemetryClient(_fakeTelemetryChannel));

        // Act
        var result = await sut.SendTemplatedEmail("template", "eventName", "Subject", _user, null);

        // Assert
        result.Should().BeFalse();
    }
}