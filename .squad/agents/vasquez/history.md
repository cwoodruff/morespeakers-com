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
