using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MoreSpeakers.Web.Tests;

public class AdminAreaAuthorizationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Anonymous_Get_Admin_Redirects_To_Login()
    {
        // Default app configuration uses cookie auth -> anonymous should be redirected to Login
        var client = factory.WithWebHostBuilder(_ => { })
            .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.GetAsync("/Admin", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        var location = resp.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Identity/Account/Login", location);
        Assert.Contains("ReturnUrl=", location);
    }

    [Fact]
    public async Task NonAdminAuthenticated_Get_Admin_Is_Forbidden_Or_AccessDenied()
    {
        var client = CreateAuthenticatedClient(isAdmin: false);

        var resp = await client.GetAsync("/Admin", TestContext.Current.CancellationToken);

        if (resp.StatusCode == HttpStatusCode.Redirect)
        {
            var location = resp.Headers.Location?.ToString() ?? string.Empty;
            Assert.Contains("/Identity/Account/AccessDenied", location);
        }
        else
        {
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }
    }

    [Fact]
    public async Task AdminAuthenticated_Get_Admin_Returns_Dashboard()
    {
        var client = CreateAuthenticatedClient(isAdmin: true);

        var html = await client.GetStringAsync("/Admin", TestContext.Current.CancellationToken);

        Assert.Contains("Admin • Dashboard", html);
    }

    private HttpClient CreateAuthenticatedClient(bool isAdmin)
    {
        var factory1 = factory.WithWebHostBuilder(builder
            => builder.ConfigureTestServices(services =>
            {
                // Register a fake authentication scheme and make it the default for tests
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                // Provide claims for the test principal via DI so the handler can read them
                services.AddScoped(_ => new TestUserContext
                {
                    Name = isAdmin ? "admin@test" : "user@test",
                    IsAdmin = isAdmin
                });
            }));

        return factory1.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private sealed class TestUserContext
    {
        public string Name { get; init; } = "test";
        public bool IsAdmin { get; init; }
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestUserContext ctx)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "TestAuth";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, ctx.Name),
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            if (ctx.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
            }

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
