# Vasquez — DataStore test harness

## Context
Issue #394 needs reusable DataStore test scaffolding before the remaining Result<T> verticals (#387, #388, #389) land.

## Decision
- Use EF Core `UseInMemoryDatabase` for the shared DataStore harness in `src\MoreSpeakers.Data.Tests\DataStoreTestBase.cs`.
  - Reason: `MoreSpeakersDbContext` carries SQL Server-specific defaults (`NEWID()`, `GETUTCDATE()`) and check constraint definitions, so SQLite in-memory would add brittle provider-specific setup to fast-running unit-style tests.
  - Trade-off: provider-enforced relational constraint failures are not covered by the harness; those cases stay in targeted tests as vertical slices adopt `Result<T>`.
- Standardize on a base-class harness pattern.
  - `DataStoreTestBase` owns fresh DbContext creation per test, AutoMapper profile wiring, logger mocks, and seeded helpers for speaker types, sectors, categories, expertises, and users.
  - New DataStore test classes should inherit the base and compose only the entity graph required for the scenario under test.
- Standardize on Bogus-backed entity builders.
  - `src\MoreSpeakers.Data.Tests\DataStoreFakers.cs` provides default generators for `User`, `Sector`, `ExpertiseCategory`, `Expertise`, and `SpeakerType` seed data.
  - Tests should override only scenario-specific fields (for example, fixed names or descriptions) and let Bogus handle uniqueness/noise for the rest.
- Standardize on Result-aware assertion helpers.
  - `src\MoreSpeakers.Data.Tests\ResultTestExtensions.cs` adds `ShouldSucceed()` and `ShouldFail()` helpers so tests assert on `Result` / `Result<T>` contracts instead of manual `IsSuccess`/`IsFailure` plumbing.

## Usage notes
- `src\MoreSpeakers.Data.Tests\ExpertiseDataStoreTests.cs` is the reference implementation for Get/GetAll/Add/Update/Delete coverage.
- Clear `Context.ChangeTracker` before update-path tests when seeding and saving through the same in-memory context; EF tracking collisions otherwise mask the Result contract being exercised.
