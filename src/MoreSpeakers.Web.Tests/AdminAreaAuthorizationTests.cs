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

public class AdminAreaAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminAreaAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_Get_Admin_Redirects_To_Login()
    {
        // Default app configuration uses cookie auth -> anonymous should be redirected to Login
        var client = _factory.WithWebHostBuilder(_ => { })
            .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        var location = resp.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Identity/Account/Login", location);
        Assert.Contains("ReturnUrl=", location);
    }

    [Fact]
    public async Task NonAdminAuthenticated_Get_Admin_Is_Forbidden_Or_AccessDenied()
    {
        var client = CreateAuthenticatedClient(isAdmin: false);

        var resp = await client.GetAsync("/Admin");

        if (resp.StatusCode == HttpStatusCode.Redirect)
        {
            var location = resp.Headers.Location?.ToString() ?? string.Empty;
            Assert.Contains("/Identity/Account/AccessDenied", location);
        }
        else
        {
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }
    }

    [Fact]
    public async Task AdminAuthenticated_Get_Admin_Returns_Dashboard()
    {
        var client = CreateAuthenticatedClient(isAdmin: true);

        var html = await client.GetStringAsync("/Admin");

        Assert.Contains("Admin • Dashboard", html);
    }

    private HttpClient CreateAuthenticatedClient(bool isAdmin)
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Register a fake authentication scheme and make it the default for tests
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                        options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });

                // Provide claims for the test principal via DI so the handler can read them
                services.AddScoped(_ => new TestUserContext
                {
                    Name = isAdmin ? "admin@test" : "user@test",
                    IsAdmin = isAdmin
                });
            });
        });

        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private sealed class TestUserContext
    {
        public string Name { get; set; } = "test";
        public bool IsAdmin { get; set; }
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string Scheme = "TestAuth";
        private readonly TestUserContext _ctx;

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            TestUserContext ctx) : base(options, logger, encoder, clock)
        {
            _ctx = ctx;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _ctx.Name),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            if (_ctx.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
            }

            var identity = new ClaimsIdentity(claims, Scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
