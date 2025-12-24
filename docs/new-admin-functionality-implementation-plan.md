# MoreSpeakers.com – Admin Feature – Implementation Overview

Based on the current solution’s architecture (ASP.NET Core 10, MVC/Razor, ASP.NET Core Identity, layered Data → Managers → Web) and what’s in the repo:
- Identity is already set up with roles; the `MoreSpeakersDbContext` seeds a role named `Administrator` (`MoreSpeakers.Data/MoreSpeakersDbContext.cs`).
- There are domain/data/manager layers in place for both Expertise and Social Media Sites:
    - Expertise: `MoreSpeakers.Domain/Models/Expertise.cs`, `MoreSpeakers.Data/Models/Expertise.cs`, `MoreSpeakers.Data/ExpertiseDataStore.cs`, `MoreSpeakers.Managers/ExpertiseManager.cs`
    - Social sites: `MoreSpeakers.Domain/Models/SocialMediaSite.cs`, `MoreSpeakers.Data/Models/SocialMediaSite.cs`, `MoreSpeakers.Data/SocialMediaSiteDataStore.cs`, `MoreSpeakers.Managers/SocialMediaSiteManager.cs`, plus Web helpers/tests
- Identity UI exists under `MoreSpeakers.Web/Areas/Identity/Pages/Account` (e.g., `Register.cshtml`), so adding a dedicated Admin area under `MoreSpeakers.Web/Areas/Admin` will align naturally with the existing structure.

This makes it straightforward to introduce an Admin area, use the `Managers` as your application boundary for CRUD, and continue to keep EF usage isolated in `Data`.

---

## High‑level Admin Plan (what to add)

1) Admin Area & Authorization
- Create an Admin area: `MoreSpeakers.Web/Areas/Admin` with MVC controllers or Razor Pages, protected by `[Authorize(Roles = Domain.Constants.UserRoles.Administrator)]`.
- Define policies to support least privilege beyond the coarse `Administrator` role (e.g., `Policy.ManageUsers`, `Policy.ManageCatalog`, `Policy.ViewReports`). Use `[Authorize(Policy = "ManageUsers")]` for sensitive actions.
- Add an Admin layout and navigation shell with links to Users, Social Sites, Expertise, Moderation, Settings, Reports.

2) User Administration
- User list and search: filter by email/username, lockout, email confirmation, role(s), date ranges.
- User detail view:
    - Profile summary: basic info, roles, last login, 2FA and lockout state.
    - Actions:
        - Reset password (generate email token or set temporary password if policy allows).
        - Force email confirmation (admin-only shortcut if business rules allow).
        - Lock/Unlock user and set lockout end date.
        - Require password reset on next sign-in.
        - Enable/disable 2FA for the account if consistent with policy.
        - Assign/remove roles (at minimum, toggle `Administrator`).
        - Soft-delete user (retain references), and optionally hard-delete (rare, if GDPR export completed).
- Bulk actions: lock, unlock, require password reset, role assignment.
- Audit trail for each sensitive action (who, what, when, why). Store in an `AdminAuditLog` table.

3) Social Media Sites Administration
- CRUD screens using `SocialMediaSiteManager`:
    - Fields: `Name` (unique), `Icon` (Font Awesome name), `UrlFormat` (required, must include a placeholder like `{handle}`), `IsActive` flag.
    - Validation:
        - Unique `Name` (db unique index + server validation).
        - `UrlFormat` must contain placeholder, and when rendered for the UI, validate the composed URL.
        - `Icon` limited to a safelist (Font Awesome names) or validated by regex.
    - List/search/paginate; toggle active; soft-delete with referential checks.
    - Preview: given a sample handle, show the rendered URL and icon preview.
- Dependency checks: prevent delete if linked to users, or offer safe migration (reassign links to another site) before delete.

4) Expertise Administration (with Sector grouping)
- Introduce a Sector grouping for expertise so related expertises can be grouped:
    - New entity `Sector` (or `ExpertiseSector`) with fields: `Id`, `Name` (unique), `Slug`, `Description`, `DisplayOrder`, `IsActive`.
    - Add `SectorId` (nullable at first, then non-nullable after migration if desired) to `Expertise` for many-to-one.
- CRUD screens:
    - Sector management: create/edit/delete sectors, control display order, toggle active.
    - Expertise management: create/edit/delete expertise, assign to a sector, toggle active.
    - Validation: unique names within the same scope, slug constraints, optional color/icon per sector for UI grouping.
- UI/UX:
    - List expertises grouped by sector; filters by sector, active status.
    - Bulk move expertises between sectors; reorder sectors and expertises (drag-and-drop optional).
- Data integrity:
    - Unique index on `Sector.Name` and `Expertise.Name` (global or per sector as you prefer).
    - Soft-delete pattern for both; restrict delete if referenced by users unless soft-deleted and hidden from selection.

5) Content Moderation & Safety (recommended)
- Reported content queue: if users can upload bios, links, or talks, allow admins to review flagged items.
- User-generated links validator (already have `SocialMediaSiteHelper.cs`): extend for admin to quickly validate all social links for a user.
- Spam/abuse controls: mark accounts as restricted, shadow-ban, or require re-verification.

6) Site Settings & Templates
- App settings editor in Admin:
    - Email templates for password reset, invite, admin notifications (subject + body, with token placeholders).
    - Feature flags (e.g., toggle new sectors UI, beta features).
    - Theming tokens that affect only admin or site.
- Safe storage for secrets remains in configuration; admin UI edits only non-secret settings persisted in DB.

7) Observability & Ops
- Admin dashboards:
    - Health checks summary (DB, email, storage)
    - Key metrics: users per week, active users, sign-in failures, locked accounts
    - Error log view (summaries), link to Application Insights if configured
- Job management:
    - Background tasks status (e.g., email send queue, cleanup jobs)
- Export/Import (GDPR friendly):
    - Export user data bundle (admin initiated or self-service assist)

8) Security & Compliance
- Authorization:
    - Keep `[Authorize(Roles = Domain.Constants.UserRoles.Administrator)]` for the area and apply granular policies to controller actions.
    - Consider a secondary role like `Moderator` for content review without user management powers.
- Input safety: server-side validation across all admin forms; anti-forgery tokens; strict model binding.
- Audit logs:
    - Capture admin actions: user changes, role assignments, deletions, sector changes, expertise edits, social site edits.
    - Store IP, admin user id, target entity, before/after snapshot (or hash/ETag), timestamp.
- Impersonation (optional, guarded): allow admins to impersonate a user to troubleshoot, with a prominent banner and audit logging.

9) UX/Usability for Admins
- Global Admin search box: search users by email/username/id, expertises by name, sectors, social sites.
- Pagination and sort controls on all list pages; filters persist via query string.
- Bulk actions with confirmation dialogs and summary of affected records.
- Keyboard-friendly nav; accessible forms and tables; consistent empty states and error messaging.

---

## Where these additions fit in the current architecture
- Web (Admin Area): controllers/pages, view models, authorization attributes, server-side validation.
- Managers layer: extend or reuse existing `ExpertiseManager` and `SocialMediaSiteManager` methods; add sector-related manager (`SectorManager`) and associated interfaces.
- Data layer: new `Sector` entity, migration(s), repositories/datastores for sectors; unique indexes and soft-delete columns; additional DTOs for admin screens if you keep domain models slim.
- Identity integration: leverage existing ASP.NET Core Identity for roles, lockouts, 2FA, password reset tokens, and email confirmation tokens.

---

## Suggested database/model changes (summary)
- New table `Sectors` (or `ExpertiseSectors`):
    - `Id` (PK), `Name` (unique), `Slug`, `Description` (nullable), `DisplayOrder` (int), `IsActive` (bool), timestamps.
- `Expertise` table: add `SectorId` (FK to `Sectors`), `IsActive` (if not present), unique index on `Name` (global or scoped).
- `SocialMediaSite` table: add `IsActive` (if not present), unique index on `Name`, constraints/lengths, keep `UrlFormat` non-null with placeholder.
- `AdminAuditLog` table: `Id`, `AdminUserId`, `Action`, `TargetType`, `TargetId`, `BeforeJson`, `AfterJson`, `CreatedAt`, `Ip`.

---

## Endpoints/pages to add (illustrative)
- Area: `Admin`
    - UsersController
        - `GET /Admin/Users` — list/search/sort
        - `GET /Admin/Users/{id}` — details
        - `POST /Admin/Users/{id}/Lock` — lock/unlock
        - `POST /Admin/Users/{id}/ResetPassword` — send token or set temp password
        - `POST /Admin/Users/{id}/Roles` — assign/remove
        - `DELETE /Admin/Users/{id}` — soft-delete
    - SocialMediaSitesController
        - `GET /Admin/SocialSites` — list/search
        - `GET /Admin/SocialSites/Create` — form
        - `POST /Admin/SocialSites/Create`
        - `GET /Admin/SocialSites/{id}/Edit`
        - `POST /Admin/SocialSites/{id}/Edit`
        - `POST /Admin/SocialSites/{id}/ToggleActive`
        - `DELETE /Admin/SocialSites/{id}` — soft-delete/migrate
    - SectorsController
        - `GET /Admin/Sectors` — list/order
        - `POST /Admin/Sectors/Create` — create
        - `POST /Admin/Sectors/{id}/Edit`
        - `POST /Admin/Sectors/{id}/Reorder`
        - `DELETE /Admin/Sectors/{id}` — restrict if in use
    - ExpertiseController
        - `GET /Admin/Expertise` — list by sector
        - `POST /Admin/Expertise/Create` — create
        - `POST /Admin/Expertise/{id}/Edit`
        - `POST /Admin/Expertise/BulkMove` — move between sectors
        - `POST /Admin/Expertise/{id}/ToggleActive`
        - `DELETE /Admin/Expertise/{id}` — soft-delete

If you prefer Razor Pages, the same routes can be expressed with pages under the Admin area.

---

## Validation rules to codify
- Social site `UrlFormat` must contain `{handle}` (or similar) and produce a valid URL when the placeholder is replaced.
- Names for Sector, Expertise, and SocialMediaSite must be unique; enforce at db and application level.
- Prevent deletion if referenced by users; prefer soft-delete and hide from selections.
- For user actions: require re-authorization for high-risk actions (policy + `IUserTwoFactorTokenProvider`), and produce audit log entries.

---

## Testing strategy
- Unit tests
    - Managers for sectors, social sites, and expertise: CRUD, validation, error paths.
    - Authorization policies: handlers accept/deny as expected.
- Integration tests
    - Admin endpoints secured by role/policy.
    - CRUD flows with in-memory or test DB (using `WebApplicationFactory`).
- UI tests (optional)
    - Happy-path flows for admin tasks; accessibility checks for forms and tables.

---

## MVC controllers (optional): applying Admin policies with Areas

Even though the Admin implementation is Razor Pages–first, you can add MVC controllers inside the Admin area later and reuse the same least‑privilege policies.

### Controller attributes
- Decorate Admin controllers with the Area attribute and the appropriate policy for that controller or action:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoreSpeakers.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ManageUsers")] // Controller-level: all actions require ManageUsers
public class UsersController : Controller
{
    // Alternatively, apply per action to mix requirements
    [HttpGet]
    public IActionResult Index() => View(); // GET /Admin/Users

    [Authorize(Policy = "ManageUsers")] // explicit (redundant here, but shows per-action usage)
    [HttpPost]
    public IActionResult Lock(Guid id) { /* ... */ return RedirectToAction("Index"); }
}

[Area("Admin")]
[Authorize(Policy = "ManageCatalog")] // Catalog/content management
public class CatalogController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(); // GET /Admin/Catalog
}

[Area("Admin")]
[Authorize(Policy = "ViewReports")] // Reports/analytics
public class ReportsController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(); // GET /Admin/Reports
}
```

Notes:
- Keep `Administrator` full access by ensuring each policy includes the `Administrator` role (already configured in `Program.cs`).
- You can mix controller‑level and action‑level `[Authorize]` depending on granularity needed.

### Route mapping for Areas (Program.cs)
If/when you introduce MVC controllers, ensure area routing is mapped in `Program.cs` in addition to `app.MapRazorPages()`:

```csharp
// After app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();

// Enable MVC area routes for Admin controllers
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // keep Razor Pages mapping
```

This preserves the Razor Pages baseline while enabling URLs like `/Admin/Users`, `/Admin/Catalog`, and `/Admin/Reports` to resolve to MVC controllers protected by the same policies.

### When to choose MVC vs Razor Pages in Admin
- Prefer Razor Pages for page‑centric CRUD workflows.
- Consider MVC controllers when you need:
    - Action‑oriented endpoints (POST‑heavy flows, AJAX/JSON APIs),
    - Complex controller filters or custom action constraints,
    - To share controllers across multiple views.

Either approach reuses the same `ManageUsers`, `ManageCatalog`, and `ViewReports` policies and the `AdminOnly` area baseline.

---

## Admin Policies and Least‑Privilege Roles (Razor Pages)

This section documents how the Admin policies are modeled, where they are registered, and how Razor Pages conventions are used so new Admin pages automatically inherit the correct, least‑privilege requirements.

### Policy definitions and intent
- `ManageUsers` — For user and role administration: list/search users, view sensitive details, lock/unlock, assign roles, reset passwords, etc.
- `ManageCatalog` — For content/catalog management: speakers, bios, tags/categories, media assets, metadata, etc.
- `ViewReports` — For analytics and operational reports: usage, health, exports, audits (view only).

Notes:
- The `Administrator` role must retain universal access and is included in all policies by design.
- Future least‑privilege roles (examples) can be mapped without changing pages:
    - `UserManager` → `ManageUsers`
    - `CatalogManager` → `ManageCatalog`
    - `Reporter` → `ViewReports`
    - `Moderator` → `ManageCatalog`, `ViewReports`

### Where and how policies are registered (Program.cs)
Policies are registered in `src/MoreSpeakers.Web/Program.cs` within the existing `AddAuthorization` block, using centralized constants in `MoreSpeakers.Web.Authorization` (`PolicyNames`, `AppRoles`).

```csharp
using MoreSpeakers.Web.Authorization;

builder.Services.AddAuthorization(options =>
{
    // Baseline Admin area policy: only Administrators can enter Admin area
    options.AddPolicy("AdminOnly", p => p.RequireRole(AppRoles.Administrator));

    // Fine‑grained Admin policies (role‑based, least privilege)
    options.AddPolicy(PolicyNames.ManageUsers,
        p => p.RequireRole(AppRoles.Administrator, AppRoles.UserManager, AppRoles.Moderator));

    options.AddPolicy(PolicyNames.ManageCatalog,
        p => p.RequireRole(AppRoles.Administrator, AppRoles.CatalogManager, AppRoles.Moderator));

    options.AddPolicy(PolicyNames.ViewReports,
        p => p.RequireRole(AppRoles.Administrator, AppRoles.Reporter, AppRoles.Moderator));

    // Administrator must remain included in every policy to preserve full access.
    // Role mappings may evolve as least‑privilege roles are introduced and seeded.

    /* Future (claims‑based) example — migrate to permission claims without
     * changing page attributes or folder conventions:
     *
     * options.AddPolicy(PolicyNames.ManageUsers,   p => p.RequireClaim("Permission", PolicyNames.ManageUsers));
     * options.AddPolicy(PolicyNames.ManageCatalog, p => p.RequireClaim("Permission", PolicyNames.ManageCatalog));
     * options.AddPolicy(PolicyNames.ViewReports,   p => p.RequireClaim("Permission", PolicyNames.ViewReports));
     *
     * Keep Administrator full access by either seeding these claims to Admin
     * or adding a custom IAuthorizationHandler that succeeds for Administrator.
     */
});
```

### Razor Pages conventions mapping folders to policies
Razor Pages conventions centralize authorization so new pages automatically inherit the correct policy based on folder placement. This is configured in `Program.cs` inside `AddRazorPages`:

```csharp
using MoreSpeakers.Web.Authorization;

builder.Services.AddRazorPages(options =>
{
    // Baseline: protect the entire Admin area
    options.Conventions.AuthorizeAreaFolder("Admin", "/", policy: "AdminOnly");

    // Sub‑folders with least‑privilege policies
    options.Conventions.AuthorizeAreaFolder("Admin", "/Users",   policy: PolicyNames.ManageUsers);
    options.Conventions.AuthorizeAreaFolder("Admin", "/Catalog", policy: PolicyNames.ManageCatalog);
    options.Conventions.AuthorizeAreaFolder("Admin", "/Reports", policy: PolicyNames.ViewReports);
});
```

Keep `Areas/Admin/Pages/Index.cshtml` (the dashboard) under `AdminOnly` only. Do not attach a granular policy to it.

### Guidance for adding new Admin pages (inheritance by folder)
Place pages under the correct sub‑folder to inherit the intended policy automatically:
- Users and role administration → `Areas/Admin/Pages/Users/*`
- Catalog/content management → `Areas/Admin/Pages/Catalog/*`
- Reports/analytics (read‑only) → `Areas/Admin/Pages/Reports/*`

Examples:
- `Areas/Admin/Pages/Users/Index.cshtml` → requires `ManageUsers`.
- `Areas/Admin/Pages/Catalog/Edit.cshtml` → requires `ManageCatalog`.
- `Areas/Admin/Pages/Reports/Usage.cshtml` → requires `ViewReports`.

### Guidance for exceptions (page‑level attributes or targeted conventions)
If a specific page needs different access than its folder default (rare), use one of:

- PageModel attribute:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Web.Authorization;

[Authorize(Policy = PolicyNames.ManageUsers)]
public class InviteModel : PageModel { /* ... */ }
```

- Targeted Razor Pages conventions in `Program.cs`:

```csharp
options.Conventions.AuthorizeAreaPage("Admin", "/Users/Invite", policy: PolicyNames.ManageUsers);
// Very rare in Admin, but supported:
options.Conventions.AllowAnonymousToAreaPage("Admin", "/Reports/Public");
```

Server‑side authorization is authoritative even if UI links are hidden/shown conditionally.

### Administrator full access and future sub‑role mapping
- `Administrator` is part of every policy and passes all checks.
- As sub‑roles are introduced, map them to policy lists (role‑based) or grant them `Permission` claims if migrating to claims:
    - `UserManager` → `ManageUsers`
    - `CatalogManager` → `ManageCatalog`
    - `Reporter` → `ViewReports`
    - `Moderator` → `ManageCatalog`, `ViewReports`

Keep names centralized in `MoreSpeakers.Web.Authorization.AppRoles` to avoid typos and drift.

### Testing approach (integration)
Integration tests use `WebApplicationFactory<Program>` with a fake authentication handler to issue principals with roles via headers. Minimal test pages exist with unique markers:
- `/Admin/Users/Test` → contains `Users Test`
- `/Admin/Catalog/Test` → contains `Catalog Test`
- `/Admin/Reports/Test` → contains `Reports Test`

Test matrix (see `MoreSpeakers.Web.Tests/AdminPoliciesTests.cs`):
- Anonymous → 302 redirect to `/Identity/Account/Login?ReturnUrl=...`
- Authenticated without relevant roles → 403 (or AccessDenied per cookie policy)
- `Administrator` → 200 and contains the page marker for all three
- `Moderator` → currently 403 for all due to the `AdminOnly` baseline; if the baseline is relaxed later, expect 200 for Catalog/Reports and 403 for Users

Example role setup in tests using headers (see `Infrastructure/TestAuthHandler`):

```csharp
var client = factory.CreateClient(new() { AllowAutoRedirect = false });
client.DefaultRequestHeaders.Add("X-Test-User", "admin");
client.DefaultRequestHeaders.Add("X-Test-Roles", AppRoles.Administrator);
```

### Future migration path to claim‑based permissions (`Permission`)
To decouple policies from specific role names and support many‑to‑many role→permission mapping:
1) Change the policy registrations to require a `Permission` claim:
    - `RequireClaim("Permission", PolicyNames.ManageUsers)` (and similarly for the others).
2) Update role/user seeding so roles grant the appropriate `Permission` claims.
3) Preserve Administrator’s universal access by either seeding all permissions to Administrator or adding a custom `IAuthorizationHandler` that succeeds for `Administrator`.
4) During migration, you may temporarily support both role‑based and claim‑based requirements to avoid cutover regressions.

This migration does not require changing Razor Pages folder conventions or page attributes.

---

## Phased delivery (pragmatic order)

Each phase will be a new issue.

1. Foundation
    - Admin area scaffold, nav shell, `[Authorize(Roles = Domain.Constants.UserRoles.Administrator)]` and granular policies
    - Admin landing dashboard with health and quick links
2. Catalog Management
    - Social media sites admin CRUD with validation and previews
    - Sector entity + migrations; sector CRUD; expertise updated to reference sectors
3. User Administration
    - Users list/detail, lock/unlock, reset password, roles, soft-delete, audit log entries
4. Moderation & Safety
    - Reported content queue, link validation tools, restricted accounts
5. Settings & Ops
    - Email templates, feature flags, basic metrics; health/reporting surfacing
6. Polish & Bulk Ops
    - Bulk actions, reordering, export/import, impersonation (if approved)

---

## Notes grounded in the repo

- `MoreSpeakers.Data/MoreSpeakersDbContext.cs` seeds role `Administrator`; use it for initial access control.
- Identity UI already lives under `MoreSpeakers.Web/Areas/Identity/Pages/Account`; reuse Identity services for user admin tasks (lockout, password reset tokens, email confirmation).
- Managers exist for Expertise and SocialMediaSite; follow the same pattern to add a `SectorManager` and any missing write methods while keeping EF inside `Data`.
- `MoreSpeakers.Web/Services/SocialMediaSiteHelper.cs` and tests indicate existing URL normalization/validation logic — extend for admin preview and validation.

