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
        Assert.True(result);
        var fakeLogRecords = _fakeLogger.Collector.GetSnapshot();
        Assert.Single(fakeLogRecords, e=>e.Level == LogLevel.Information && e.Message == $"eventName email was successfully sent to {_user.Email}");
    }

    [Fact]
    public async Task SendTemplatedEmail_LogsException()
    {
        _stringRendererMock.Setup(mock => mock.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new InvalidOperationException("Rendering failed"));
        var result = await _templatedEmailSender.SendTemplatedEmail("template", "eventName", "subject", _user, null);
        Assert.False(result);
        var fakeLogRecords = _fakeLogger.Collector.GetSnapshot();
        Assert.Single(fakeLogRecords, e=>e.Level == LogLevel.Error && e.Message == $"Failed to send eventName email to {_user.Email}");
    }

    [Fact]
    public async Task SendTemplatedEmail_EmitsSuccessTelemetry()
    {
        var result = await _templatedEmailSender.SendTemplatedEmail("template", "eventName", "subject", _user, null);
        Assert.True(result);
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
}