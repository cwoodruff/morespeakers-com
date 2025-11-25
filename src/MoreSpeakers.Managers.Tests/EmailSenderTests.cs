using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

using Azure.Storage.Queues;

namespace MoreSpeakers.Managers.Tests;

public class EmailSenderTests
{
    [Fact(Skip = "EmailSender requires Azure Queue Service and JosephGuadagno.AzureHelpers.Queue which are not easily mockable here. Recommend covering with integration tests or abstracting queue interactions behind an interface for unit testing.")]
    public async Task QueueEmail_should_enqueue_message()
    {
        var qsc = new QueueServiceClient("UseDevelopmentStorage=true");
        var settings = new Mock<ISettings>();
        settings.SetupGet(s => s.Email).Returns(new EmailSettings
        {
            FromAddress = "from@example.com",
            FromName = "From",
            ReplyToAddress = "reply@example.com",
            ReplyToName = "Reply"
        });
        var logger = new Mock<ILogger<EmailSender>>();
        var sut = new EmailSender(qsc, settings.Object, logger.Object);

        await sut.QueueEmail(new MailAddress("to@example.com"), "subject", "body");
    }

    [Fact(Skip = "See comment in previous test. This verifies SendEmailAsync delegates to QueueEmail but requires the same external Azure Queue dependency.")]
    public async Task SendEmailAsync_should_delegate_to_queue_email()
    {
        var qsc = new QueueServiceClient("UseDevelopmentStorage=true");
        var settings = new Mock<ISettings>();
        settings.SetupGet(s => s.Email).Returns(new EmailSettings
        {
            FromAddress = "from@example.com",
            FromName = "From",
            ReplyToAddress = "reply@example.com",
            ReplyToName = "Reply"
        });
        var logger = new Mock<ILogger<EmailSender>>();
        var sut = new EmailSender(qsc, settings.Object, logger.Object);

        await sut.SendEmailAsync("to@example.com", "subject", "<p>hi</p>");
    }
}
