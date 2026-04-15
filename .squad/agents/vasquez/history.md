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
- 2026-04-15: Orchestration log archived. Inbox decisions merged to decisions.md. Session log written. User directives captured (exception standardization, branch naming).

## Learnings

- The existing testing toolbox is xUnit, FluentAssertions, Moq, and Bogus.
- Issue #385 contract coverage lives in `src/MoreSpeakers.Domain.Tests\ResultFoundationTests.cs` and uses reflection against `MoreSpeakers.Domain` so tests compile before `Result`, `Result<T>`, and `Error` exist.
- Reflection-based contract tests allow test-first iteration on large surface area without blocking on compile gates.
- Expertise Result<T> coverage now lives in `src\MoreSpeakers.Managers.Tests\ExpertiseManagerTests.cs`, `src\MoreSpeakers.Web.Tests\Areas\Admin\Pages\Catalog\Expertises\**\*.cs`, and `src\MoreSpeakers.Data.Tests\ExpertiseDataStoreResultTests.cs`.
- `src\MoreSpeakers.Data.Tests\MoreSpeakers.Data.Tests.csproj` needs `xunit.runner.visualstudio` for `dotnet test` to discover xUnit v3 tests in this repo.
- Expertise manager validation currently normalizes names/descriptions and returns manager-level error codes like `expertise.validation.name-required` and `expertise.validation.invalid-id`.

## Issue #386 Completion (2026-04-15)

- Issue #386 Expertise Result<T> test coverage completed. Rewired Expertise manager tests for Result success/failure paths and admin page tests for ModelState/TempData handling. Added DataStore Result coverage to MoreSpeakers.Data.Tests. Fixed critical infrastructure issue: Data.Tests project had zero executable tests until `xunit.runner.visualstudio` dependency added. Final validation: Managers 14/14, Web 48/48, Data 9/9. Data.Tests now fully operational for incremental test harness expansion (#394) across remaining verticals.
