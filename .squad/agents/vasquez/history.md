# Project Context

- **Project:** morespeakers-com
- **Owner:** Joseph Guadagno
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, HTMX, Azure Functions, xUnit, FluentAssertions, Moq, Bogus, GitHub Actions, Azure

## Core Context

- Owns test strategy and reviewer gates for the MoreSpeakers squad.
- Focused on xUnit-based coverage with FluentAssertions, Moq, and Bogus.

## Recent Updates

- 2026-04-13: Team initialized with the Alien cast.
- 2026-04-15: Contract-first test suite completed for Issue #385 foundation. Reflection-based tests allow compilation before domain types exist. Full coverage: Result, Result<T>, Error types, factory methods, implicit conversions, equality. Tests serve as contract gate for implementation.
- 2026-05-24: **Result<T> foundation implementation complete (PR #406 merged).** Dallas delivered Domain/Result.cs, Result<T>.cs, Error.cs. All 7 foundation tests passing. API shape documented in decisions.md. Ready to begin work on #394 (DataStore test harness) and any other assigned issues.
- 2026-04-15: Orchestration log archived. Inbox decisions merged to decisions.md. Session log written. User directives captured (exception standardization, branch naming).

## Learnings

- The existing testing toolbox is xUnit, FluentAssertions, Moq, and Bogus.
- Issue #385 contract coverage lives in `src/MoreSpeakers.Domain.Tests\ResultFoundationTests.cs` and uses reflection against `MoreSpeakers.Domain` so tests compile before `Result`, `Result<T>`, and `Error` exist.
- Reflection-based contract tests allow test-first iteration on large surface area without blocking on compile gates.
- Expertise Result<T> coverage now lives in `src\MoreSpeakers.Managers.Tests\ExpertiseManagerTests.cs`, `src\MoreSpeakers.Web.Tests\Areas\Admin\Pages\Catalog\Expertises\**\*.cs`, and `src\MoreSpeakers.Data.Tests\ExpertiseDataStoreResultTests.cs`.
- `src\MoreSpeakers.Data.Tests\MoreSpeakers.Data.Tests.csproj` needs `xunit.runner.visualstudio` for `dotnet test` to discover xUnit v3 tests in this repo.
- Expertise manager validation currently normalizes names/descriptions and returns manager-level error codes like `expertise.validation.name-required` and `expertise.validation.invalid-id`.
- Issue #394 test harness scaffolding now lives in `src\MoreSpeakers.Data.Tests\DataStoreTestBase.cs`, `DataStoreFakers.cs`, and `ResultTestExtensions.cs`; new DataStore tests should inherit the base harness instead of re-creating DbContext/mapper setup.
- `src\MoreSpeakers.Data.Tests\ExpertiseDataStoreTests.cs` establishes the pattern: seed minimal sector/category graphs with Bogus-backed helpers, clear the EF change tracker before update-path assertions, and assert `Result` success/failure via `ShouldSucceed()` / `ShouldFail()` helpers.

## Issue #386 Completion (2026-04-15)

- Issue #386 Expertise Result<T> test coverage completed. Rewired Expertise manager tests for Result success/failure paths and admin page tests for ModelState/TempData handling. Added DataStore Result coverage to MoreSpeakers.Data.Tests. Fixed critical infrastructure issue: Data.Tests project had zero executable tests until `xunit.runner.visualstudio` dependency added. Final validation: Managers 14/14, Web 48/48, Data 9/9. Data.Tests now fully operational for incremental test harness expansion (#394) across remaining verticals.

## Issue #384 Completion (2026-05-22)

- Issue #384 xUnit version sync completed. Standardized all test projects to xUnit v3 (3.2.1). Key changes: upgraded MoreSpeakers.Managers.Tests from xUnit v2 (2.9.3) to v3, added FluentAssertions to Data.Tests for consistency, added OutputType=Exe for v3 compatibility. All 262 tests pass. Final versions: xunit.v3 3.2.1, xunit.runner.visualstudio 3.1.5, Microsoft.NET.Test.Sdk 18.0.1, FluentAssertions 8.8.0, Moq 4.20.72. PR #404 created.

## Issue #390 Test Gap Analysis (2026-05-24)

- **Audit Summary:** Completed test coverage audit for 4 services Dallas is converting to Result<T> on branch `issue-390-result-t-final-cleanup`. GitHubService (6 tests), TemplatedEmailSender (6 tests), and OpenGraphSpeakerProfileImageGenerator (~25 tests) have existing coverage that needs Result<T> rewiring. **CRITICAL GAP:** FileUploadService has ZERO tests and `[ExcludeFromCodeCoverage]` attribute. Comprehensive test plan created: 3 new tests for GitHubService, 4 for TemplatedEmailSender, 9 for OpenGraphSpeakerProfileImageGenerator (including new QueueSpeakerOpenGraphProfileImageCreation coverage), and 15 from scratch for FileUploadService. Test infrastructure ready: ResultTestExtensions with `.ShouldSucceed()` / `.ShouldFail("error-code")` patterns established. Test plan documented in `.squad/decisions/inbox/vasquez-390-test-plan.md`. Blocked on branch availability from Dallas.
