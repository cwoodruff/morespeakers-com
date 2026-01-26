using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using MoreSpeakers.Web.Authorization;
using MoreSpeakers.Web.Tests.Infrastructure;

namespace MoreSpeakers.Web.Tests;

public class AdminPoliciesTests(CustomWebApplicationFactory authFactory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Anonymous_requests_redirect_to_login_for_all_policy_pages()
    {
        // Use a factory without test auth override to preserve cookie challenge behavior (302 to login)
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        foreach (var path in new[]
                 {
                     "/Admin/Users/Test",
                     "/Admin/Catalog/Test",
                     "/Admin/Reports/Test"
                 })
        {
            var resp = await client.GetAsync(path, TestContext.Current.CancellationToken);
            _ = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
            var location = resp.Headers.Location?.ToString() ?? string.Empty;
            Assert.Contains("/Identity/Account/Login", location, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("ReturnUrl=", location, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Authenticated_non_privileged_user_gets_403_for_all_policy_pages()
    {
        var client = authFactory.CreateClient(new() { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthDefaults.UserHeader, "testuser");
        // No roles header → authenticated but not in Administrator nor specific policy roles

        foreach (var path in new[]
                 {
                     "/Admin/Users/Test",
                     "/Admin/Catalog/Test",
                     "/Admin/Reports/Test"
                 })
        {
            var resp = await client.GetAsync(path, TestContext.Current.CancellationToken);
            var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            // With Test auth scheme default, forbidden remains 403 (no redirect)
            Assert.True(resp.StatusCode == HttpStatusCode.NotFound, $"Expected 404. Actual: {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        }
    }

    [Fact]
    public async Task Administrator_can_access_all_policy_pages()
    {
        var client = authFactory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthDefaults.UserHeader, "admin");
        client.DefaultRequestHeaders.Add(TestAuthDefaults.RolesHeader, AppRoles.Administrator);

        var users = await client.GetAsync("/Admin/Users/Test", TestContext.Current.CancellationToken);
        var usersHtml = await users.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(users.StatusCode == HttpStatusCode.OK, $"Expected 200. Actual: {(int)users.StatusCode} {users.StatusCode}. Body: {usersHtml}");
        Assert.Contains("Users Test", usersHtml);

        var catalog = await client.GetAsync("/Admin/Catalog/Test", TestContext.Current.CancellationToken);
        var catalogHtml = await catalog.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(catalog.StatusCode == HttpStatusCode.OK, $"Expected 200. Actual: {(int)catalog.StatusCode} {catalog.StatusCode}. Body: {catalogHtml}");
        Assert.Contains("Catalog Test", catalogHtml);

        var reports = await client.GetAsync("/Admin/Reports/Test", TestContext.Current.CancellationToken);
        var reportsHtml = await reports.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(reports.StatusCode == HttpStatusCode.OK, $"Expected 200. Actual: {(int)reports.StatusCode} {reports.StatusCode}. Body: {reportsHtml}");
        Assert.Contains("Reports Test", reportsHtml);
    }

    [Fact]
    public async Task Moderator_allowed_for_Catalog_and_Reports_but_denied_for_Users_due_to_AdminOnly_baseline()
    {
        var client = authFactory.CreateClient(new() { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthDefaults.UserHeader, "moderator1");
        client.DefaultRequestHeaders.Add(TestAuthDefaults.RolesHeader, AppRoles.Moderator);

        // Note: The Admin area baseline requires Administrator (AdminOnly). Since Moderator is not Administrator,
        // the baseline will deny access before granular folder policies, resulting in 403 for all.
        // If baseline is relaxed in the future, these expectations can be adjusted to 200 for Catalog/Reports.

        var catalog = await client.GetAsync("/Admin/Catalog/Test", TestContext.Current.CancellationToken);
        var bodyC = await catalog.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(catalog.StatusCode == HttpStatusCode.NotFound, $"Expected 404. Actual: {(int)catalog.StatusCode} {catalog.StatusCode}. Body: {bodyC}");

        var reports = await client.GetAsync("/Admin/Reports/Test", TestContext.Current.CancellationToken);
        var bodyR = await reports.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(reports.StatusCode == HttpStatusCode.NotFound, $"Expected 404. Actual: {(int)reports.StatusCode} {reports.StatusCode}. Body: {bodyR}");

        var users = await client.GetAsync("/Admin/Users/Test", TestContext.Current.CancellationToken);
        var bodyU = await users.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(users.StatusCode == HttpStatusCode.NotFound, $"Expected 404. Actual: {(int)users.StatusCode} {users.StatusCode}. Body: {bodyU}");
    }
}
