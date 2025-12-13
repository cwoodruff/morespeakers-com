# Admin Area — Overview and How‑To

## Purpose and Role Requirement

The Admin area provides a secured space for site administration tasks (user management, content moderation, site settings, etc.).

- Access requirement: Users must be authenticated and in the `Administrator` role.
- All Razor Pages under `Areas/Admin/Pages/**` are protected by the `AdminOnly` authorization policy.


## Folder Structure and Key Files

Admin area follows Razor Pages conventions under the web project.

```
MoreSpeakers.Web/
  Areas/
    Admin/
      Pages/
        _ViewStart.cshtml                 # Sets Admin layout for all Admin pages
        _ViewImports.cshtml               # Common imports/tag helpers for Admin pages
        Index.cshtml                      # Admin dashboard landing page (placeholder)
        Shared/
          _Layout.Admin.cshtml            # Admin layout and shell navigation
```

Key files:
- `Areas/Admin/Pages/_ViewStart.cshtml`
    - Sets the layout for all Admin pages:
      ```cshtml
      @{
          Layout = "~/Areas/Admin/Pages/Shared/_Layout.Admin.cshtml";
      }
      ```
- `Areas/Admin/Pages/_ViewImports.cshtml`
    - Makes ASP.NET Core Tag Helpers available and brings common namespaces into scope:
      ```cshtml
      @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
      @using MoreSpeakers.Web
      @using MoreSpeakers.Domain
      ```
- `Areas/Admin/Pages/Shared/_Layout.Admin.cshtml`
    - Admin layout shell: distinct title/branding, top nav (Dashboard/Users/Content/Settings placeholders), and profile/sign‑out area.
- `Areas/Admin/Pages/Index.cshtml`
    - Admin landing page (Dashboard placeholder) with minimal KPI cards and quick links; uses the Admin layout via `_ViewStart`.


## Authorization Configuration (Policy + Conventions)

Authorization is configured in `Program.cs` using a named policy applied to the Admin area via Razor Pages conventions.

Policy definition:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
});
```

Razor Pages area convention:
```csharp
builder.Services.AddRazorPages(options =>
{
    // Protect all Admin pages with the AdminOnly policy
    options.Conventions.AuthorizeAreaFolder("Admin", "/", policy: "AdminOnly");
});
```

Cookie paths (login/access denied) are configured to Identity UI defaults so anonymous users are redirected to Login and non‑admins to AccessDenied:
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
```

Routing: Razor Pages requires only `app.MapRazorPages();` (already present). No additional area route mapping is required for Razor Pages.


### Granting the `Administrator` Role to a User

Prerequisites:
- Roles are enabled and registered: `.AddRoles<IdentityRole<Guid>>()` in Identity setup.
- The `Administrator` role is seeded/created (seeding exists in the data layer; if not present in an environment, create it once).

Options to grant a user the role:
1) Application code (one‑time seeding or admin tool)
   ```csharp
   // inside a scoped block after app start (e.g., a hosted service or startup task)
   using var scope = app.Services.CreateScope();
   var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
   var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MoreSpeakers.Data.Models.User>>();

   const string adminRole = "Administrator";
   if (!await roleManager.RoleExistsAsync(adminRole))
       await roleManager.CreateAsync(new IdentityRole<Guid>(adminRole));

   var user = await userManager.FindByEmailAsync("someone@example.com");
   if (user is not null && !await userManager.IsInRoleAsync(user, adminRole))
       await userManager.AddToRoleAsync(user, adminRole);
   ```

2) Immediate script in a temporary console/controller endpoint (for local dev only)
    - Resolve `UserManager<MoreSpeakers.Data.Models.User>` and `RoleManager<IdentityRole<Guid>>` and call `AddToRoleAsync` as above.

3) Database seed/migration
    - If you use custom seeding, ensure it creates the `Administrator` role and assigns it to a known admin user in non‑production environments.

Notes:
- Replace `someone@example.com` with the target account.
- Avoid exposing a public endpoint for this in production; prefer controlled scripts or admin tooling with proper authorization.


### Adding New Admin Pages (Conventions and Link Patterns)

New Razor Pages should live under `Areas/Admin/Pages`. Examples:

- Create a page under a feature folder, e.g., `Areas/Admin/Pages/Users/Index.cshtml`:
  ```cshtml
  @page
  @model Users_IndexModel
  @{ ViewData["Title"] = "Admin • Users"; }
  <h1 class="h3">Users</h1>
  ```

- Link to Admin pages using Razor Tag Helpers to preserve area routing:
    - From anywhere (site or admin):
      ```cshtml
      <a asp-area="Admin" asp-page="/Index">Admin Dashboard</a>
      <a asp-area="Admin" asp-page="/Users/Index">Manage Users</a>
      ```

- Layout and imports are inherited automatically:
    - `_ViewStart.cshtml` applies the Admin layout to all pages in the area.
    - `_ViewImports.cshtml` enables Tag Helpers and common namespaces.

- Authorization:
    - All pages under `Areas/Admin/Pages/**` are already protected by the `AdminOnly` policy via conventions; no need to add `[Authorize]` attributes on each page.


## Verification (Acceptance Criteria)

- Anonymous request to `/Admin` → redirected to `/Identity/Account/Login`.
- Authenticated non‑admin → redirected to `/Identity/Account/AccessDenied` (403 semantics).
- Authenticated `Administrator` → sees Admin dashboard with Admin layout and nav placeholders.


## Helpful UI References

- Main site layout conditionally shows an Admin link only to administrators (`Pages/Shared/_Layout.cshtml`).
- Admin layout file: `Areas/Admin/Pages/Shared/_Layout.Admin.cshtml`.


## Future Enhancements

- Add a user dropdown in the Admin header with links to Manage Profile and Sign out.
- Add breadcrumbs (e.g., Admin > Dashboard) and reusable admin partials.
- Add telemetry (Application Insights/Serilog properties) to tag Admin requests and pages.
