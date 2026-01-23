using System.Net;
using System.Text;
using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.DTOs;

namespace MoreSpeakers.Managers.Tests;

public class GitHubServiceTests
{
    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return await _handler(request);
        }
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("boom");
    }

    private static ISettings CreateSettings()
    {
        var settings = new Mock<ISettings>();
        settings.SetupGet(s => s.GitHub).Returns(new GitHubSettings
        {
            RepoOwner = "owner",
            RepoName = "repo",
            CacheKey = "github-cache",
            CacheDurationInMinutes = 5
        });
        return settings.Object;
    }

    [Fact]
    public async Task GetContributorsAsync_returns_cached_value_when_available()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var expected = new List<GitHubContributor> { new() { Login = "cached" } };
        cache.Set("github-cache", expected);
        var httpClient = new HttpClient(new TestHttpMessageHandler(_ => throw new InvalidOperationException("http")));
        var logger = new Mock<ILogger<GitHubService>>();
        var sut = new GitHubService(httpClient, cache, CreateSettings(), logger.Object);

        var result = await sut.GetContributorsAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetContributorsAsync_returns_empty_when_cache_entry_is_null()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set<object?>("github-cache", null);
        var httpClient = new HttpClient(new TestHttpMessageHandler(_ => throw new InvalidOperationException("http")));
        var logger = new Mock<ILogger<GitHubService>>();
        var sut = new GitHubService(httpClient, cache, CreateSettings(), logger.Object);

        var result = await sut.GetContributorsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetContributorsAsync_fetches_and_caches_contributors()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var contributors = new List<GitHubContributor>
        {
            new() { Login = "one", AvatarUrl = "a", HtmlUrl = "h", Contributions = 1 }
        };
        var json = JsonSerializer.Serialize(contributors);
        var handler = new TestHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });
        var httpClient = new HttpClient(handler);
        var logger = new Mock<ILogger<GitHubService>>();
        var sut = new GitHubService(httpClient, cache, CreateSettings(), logger.Object);

        var result = (await sut.GetContributorsAsync()).ToList();

        result.Should().BeEquivalentTo(contributors);
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.ToString().Should().Be("https://api.github.com/repos/owner/repo/contributors");
        handler.LastRequest.Headers.UserAgent.ToString().Should().Contain("MoreSpeakers-App");
        cache.TryGetValue("github-cache", out IEnumerable<GitHubContributor>? cached).Should().BeTrue();
        cached.Should().BeEquivalentTo(contributors);
    }

    [Fact]
    public async Task GetContributorsAsync_returns_empty_when_api_returns_null()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new TestHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });
        var httpClient = new HttpClient(handler);
        var logger = new Mock<ILogger<GitHubService>>();
        var sut = new GitHubService(httpClient, cache, CreateSettings(), logger.Object);

        var result = await sut.GetContributorsAsync();

        result.Should().BeEmpty();
        cache.TryGetValue("github-cache", out _).Should().BeFalse();
    }

    [Fact]
    public async Task GetContributorsAsync_logs_error_and_returns_empty_on_exception()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var httpClient = new HttpClient(new ThrowingHttpMessageHandler());
        var logger = new Mock<ILogger<GitHubService>>();
        var sut = new GitHubService(httpClient, cache, CreateSettings(), logger.Object);

        var result = await sut.GetContributorsAsync();

        result.Should().BeEmpty();
        logger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Error getting GitHub contributors from")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
