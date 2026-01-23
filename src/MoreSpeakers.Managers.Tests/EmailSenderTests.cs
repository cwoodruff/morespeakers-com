using System.Net;
using System.Net.Mail;
using System.Text;

using Azure.Core.Pipeline;
using Azure.Storage;
using Azure.Storage.Queues;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers.Tests;

public class EmailSenderTests
{
    private sealed class FakeQueueHandler : HttpMessageHandler
    {
        private const string SendMessageResponseXml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<QueueMessagesList>" +
            "<QueueMessage>" +
            "<MessageId>11111111-1111-1111-1111-111111111111</MessageId>" +
            "<InsertionTime>Wed, 01 Jan 2025 00:00:00 GMT</InsertionTime>" +
            "<ExpirationTime>Wed, 01 Jan 2025 01:00:00 GMT</ExpirationTime>" +
            "<PopReceipt>pop</PopReceipt>" +
            "<TimeNextVisible>Wed, 01 Jan 2025 00:01:00 GMT</TimeNextVisible>" +
            "</QueueMessage>" +
            "</QueueMessagesList>";

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content != null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }
            var statusCode = request.Method == HttpMethod.Post || request.Method == HttpMethod.Put
                ? HttpStatusCode.Created
                : HttpStatusCode.OK;
            var response = new HttpResponseMessage(statusCode)
            {
                Content = request.Method == HttpMethod.Head
                    ? null
                    : new StringContent(SendMessageResponseXml, Encoding.UTF8, "application/xml")
            };
            return response;
        }
    }

    private static QueueServiceClient CreateQueueServiceClient(FakeQueueHandler handler)
    {
        var options = new QueueClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(handler))
        };
        var credential = new StorageSharedKeyCredential("testaccount", Convert.ToBase64String(new byte[32]));
        return new QueueServiceClient(new Uri("https://example.com"), credential, options);
    }

    private static ISettings CreateSettings()
    {
        var settings = new Mock<ISettings>();
        settings.SetupGet(s => s.Email).Returns(new EmailSettings
        {
            FromAddress = "from@example.com",
            FromName = "From",
            ReplyToAddress = "reply@example.com",
            ReplyToName = "Reply"
        });
        return settings.Object;
    }

    [Fact]
    public async Task QueueEmail_should_enqueue_message()
    {
        var handler = new FakeQueueHandler();
        var qsc = CreateQueueServiceClient(handler);
        var logger = new Mock<ILogger<EmailSender>>();
        var sut = new EmailSender(qsc, CreateSettings(), logger.Object);

        await sut.QueueEmail(new MailAddress("to@example.com", "To User"), "subject", "body");

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequestBody.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendEmailAsync_should_delegate_to_queue_email()
    {
        var handler = new FakeQueueHandler();
        var qsc = CreateQueueServiceClient(handler);
        var logger = new Mock<ILogger<EmailSender>>();
        var sut = new EmailSender(qsc, CreateSettings(), logger.Object);

        await sut.SendEmailAsync("to@example.com", "subject", "<p>hi</p>");

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequestBody.Should().NotBeNullOrEmpty();
    }
}
