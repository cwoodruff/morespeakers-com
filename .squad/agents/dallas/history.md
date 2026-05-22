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

- 2026-05-22: Issue #393 Console.WriteLine fix completed. Pattern: This codebase uses `[LoggerMessage]` source-generated logging. Classes using ILogger have a corresponding `.logger.cs` partial file (e.g., `OpenGraphSpeakerProfileImageGenerator.cs` + `OpenGraphSpeakerProfileImageGenerator.logger.cs`). Logger methods are defined as `static partial void` with `[LoggerMessage(LogLevel, "message template {Parameter}")]` attribute. For logging exceptions, add `Exception exception` as the last parameter. The source generator automatically creates the implementation. Location: `src\MoreSpeakers.Managers\OpenGraphSpeakerProfileImageGenerator.logger.cs`.

- 2026-05-22: Reviewed PRs #400 (Html.Raw XSS) and #401 (innerHTML XSS). Both are correct and safe to merge. Key finding: `_UserList.cshtml` has 6 `@Html.Raw(CaretFor(...))` calls — these are safe (CaretFor returns only hardcoded icon markup, never user data) and are out of scope for PR #400. In PR #401, one residual `innerHTML` in register.js (`button.innerHTML = originalContent`) remains; it is server-rendered Razor content and accepted risk. Decision: `@Html.Raw()` is acceptable only for server-controlled static HTML markup (e.g., icon helpers); never for any value derived from user input.

- 2026-05-22: PR #402 test failure investigation revealed a pre-existing CI configuration issue, not related to the Console.WriteLine → LoggerMessage changes. The ApplicationInsights connection string configuration in Program.cs (line 247-248) was attempting to parse placeholder values in the CI environment, causing test failures across all PRs. Fixed by adding null/whitespace checks and skipping ApplicationInsights configuration when the connection string is invalid or a placeholder. All 262 tests pass locally. The original logger changes (OpenGraphSpeakerProfileImageGenerator using [LoggerMessage] instead of Console.WriteLine) are correct and working as intended.

- This project includes both an ASP.NET Razor Pages application and an Azure Functions project.
- 2026-04-15: Phase 1 Result foundation lives in `src\MoreSpeakers.Domain\Models\Error.cs` and `src\MoreSpeakers.Domain\Models\Result.cs`, with coverage in `src\MoreSpeakers.Domain.Tests\ResultTests.cs`.
- 2026-04-15: The Result pattern here keeps creation on static `Result` factory methods to mirror `IdentityResult`, while `Result<T>` adds implicit value conversion and explicit failure accessors.
- 2026-04-15: Expertise PoC converted the core slice to `Result` contracts in `src\MoreSpeakers.Domain\Interfaces\IExpertiseDataStore.cs` and `IExpertiseManager.cs`, with manager coverage in `src\MoreSpeakers.Managers.Tests\ExpertiseManagerTests.cs`.
- 2026-04-15: For Expertise, expected persistence failures now return structured `Error` codes from `src\MoreSpeakers.Data\ExpertiseDataStore.cs`, while only `DbUpdateException` is caught and broader exceptions are allowed to bubble.
- 2026-04-15: Issue #386 Expertise Result<T> PoC completed. Full vertical slice Domain/Data/Manager/Web migrated to Result pattern. Test coverage wired for success/failure paths across all layers. Expertise DataStore enforces explicit Result failures for missing records; Manager adds boundary validation only. Web established server-rendered error patterns (inline ModelState + TempData toast). Orchestration and session logs archived. Decisions merged. Ready for downstream Phase 3 verticals (#387–#389) and cleanup (#390).
