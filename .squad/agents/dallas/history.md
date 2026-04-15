# Project Context

- **Project:** morespeakers-com
- **Owner:** Joseph Guadagno
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, HTMX, Azure Functions, xUnit, FluentAssertions, Moq, Bogus, GitHub Actions, Azure

## Core Context

- Owns backend implementation across the web app and Azure Functions.
- Keeps service and integration work aligned with the rest of the squad.

## Recent Updates

- 2026-04-13: Team initialized with the Alien cast.
- 2026-04-15: Phase 1 Result foundation complete; types added to Domain, mirroring IdentityResult factory pattern. Public namespace export via GlobalUsings. Ready for Phase 2 PoC (#386).
- 2026-04-15: Orchestration log archived. Inbox decisions merged to decisions.md. Session log written. User directives captured (exception standardization, branch naming).

## Learnings

- This project includes both an ASP.NET Razor Pages application and an Azure Functions project.
- 2026-04-15: Phase 1 Result foundation lives in `src\MoreSpeakers.Domain\Models\Error.cs` and `src\MoreSpeakers.Domain\Models\Result.cs`, with coverage in `src\MoreSpeakers.Domain.Tests\ResultTests.cs`.
- 2026-04-15: The Result pattern here keeps creation on static `Result` factory methods to mirror `IdentityResult`, while `Result<T>` adds implicit value conversion and explicit failure accessors.
- 2026-04-15: Expertise PoC converted the core slice to `Result` contracts in `src\MoreSpeakers.Domain\Interfaces\IExpertiseDataStore.cs` and `IExpertiseManager.cs`, with manager coverage in `src\MoreSpeakers.Managers.Tests\ExpertiseManagerTests.cs`.
- 2026-04-15: For Expertise, expected persistence failures now return structured `Error` codes from `src\MoreSpeakers.Data\ExpertiseDataStore.cs`, while only `DbUpdateException` is caught and broader exceptions are allowed to bubble.
- 2026-04-15: Issue #386 Expertise Result<T> PoC completed. Full vertical slice Domain/Data/Manager/Web migrated to Result pattern. Test coverage wired for success/failure paths across all layers. Expertise DataStore enforces explicit Result failures for missing records; Manager adds boundary validation only. Web established server-rendered error patterns (inline ModelState + TempData toast). Orchestration and session logs archived. Decisions merged. Ready for downstream Phase 3 verticals (#387–#389) and cleanup (#390).
