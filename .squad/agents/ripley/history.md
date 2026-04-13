# Project Context

- **Project:** morespeakers-com
- **Owner:** Joseph Guadagno
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, HTMX, Azure Functions, xUnit, FluentAssertions, Moq, Bogus, GitHub Actions, Azure

## Core Context

- Lead for the initial MoreSpeakers squad.
- Coordinate work that crosses the Razor Pages app, Azure Functions, tests, and delivery pipeline.

## Recent Updates

- 2026-04-13: Team initialized with the Alien cast.
- 2026-04-13: Completed codebase quality review (security, code quality, testability, exception handling). Findings awaiting squad consensus.

## Learnings

- Joseph wants the squad oriented around server-rendered UX, backend logic, testing, and Azure deployment.
- 2026-07-16: Completed project structure audit and README reconciliation.
- 2025-07-18: Completed full codebase quality review (security, code quality, testability, exception handling). Key findings: XSS via Html.Raw in profile edit partials, innerHTML XSS in JS files, 35+ generic catch(Exception) blocks, zero DataStore test coverage, Console.WriteLine in production code. CSRF finding was false positive (Razor Pages auto-validate). SQL script passwords are dev-only (Aspire container).

### Architecture (verified)
- **Dependency graph:** Web → {Managers, Data, Domain, ServiceDefaults} | Managers → Domain | Data → Domain | AppHost → {Web, Functions}
- **Data layer pattern:** EF Core is used as a runtime ORM with AutoMapper for Data↔Domain model mapping. Schema is managed via raw SQL scripts loaded by Aspire, NOT EF migrations.
- **Service registration:** DataStores and Managers are registered as `Scoped` in `Program.cs`. Each domain entity has a matching interface pair (e.g., `IExpertiseDataStore`/`IExpertiseManager`) defined in `MoreSpeakers.Domain.Interfaces`.
- **Logging pattern:** Managers and DataStores use `partial class` with a separate `.logger.cs` file containing `[LoggerMessage]` source-generated log methods.
- **Test layout:** Four test projects mirror the four library projects. Tests use xUnit, FluentAssertions, Moq. Manager tests mock DataStore interfaces.
- **Admin area:** Uses role-based authorization with policies (AdminOnly, ManageUsers, ManageCatalog, ViewReports). Defined in `Web/Authorization/`.
- **Frontend:** Razor Pages + HTMX + Hyperscript. No SPA frameworks. Static files managed by LibMan.
- **Azure Functions:** Handle email delivery (SendGrid), OpenGraph image generation, poison message handling.
- **SQL scripts:** `scripts/database/` contains: create-database, create-tables, create-views, create-functions, seed-data, plus dated migration scripts.

### Key Paths
- Solution: `src/MoreSpeakers.sln`
- AppHost entry: `src/MoreSpeakers.AppHost/AppHost.cs`
- Web entry: `src/MoreSpeakers.Web/Program.cs`
- DbContext: `src/MoreSpeakers.Data/MoreSpeakersDbContext.cs`
- Domain interfaces: `src/MoreSpeakers.Domain/Interfaces/`
- Domain models: `src/MoreSpeakers.Domain/Models/`
- SQL scripts: `scripts/database/`
- Build command: `dotnet build` from `src/`
- Test command: `dotnet test` from `src/`
