---
name: "result-foundation"
description: "Add or extend MoreSpeakers Domain Result types with explicit factory methods and structured errors."
domain: "error-handling"
confidence: "high"
source: "earned"
---

## Context
Use this skill when implementing or extending the approved Result-based exception handling migration in the Domain layer. It applies when expected failures need to flow through Data, Managers, and Web without losing structured context.

## Patterns
- Keep `Error`, `Result`, and `Result<T>` in `src\MoreSpeakers.Domain\Models\`, but expose them from the root `MoreSpeakers.Domain` namespace.
- Model creation after `IdentityResult`: factory methods live on non-generic `Result` instead of exposing public constructors.
- Use `readonly struct` for `Result` and `Result<T>` to stay additive and allocation-conscious on the hot path.
- Keep failure access explicit: success results throw if callers read `Error`, and failure results throw if callers read `Value`.
- Cover the public API surface in `src\MoreSpeakers.Domain.Tests\ResultTests.cs`, including success, failure, implicit conversion, and equality.
- When converting an existing vertical slice, let the slice interface own explicit `Result` signatures instead of inheriting generic `IDataStore` contracts that force sentinel returns.
- In DataStores, catch only expected persistence exceptions such as `DbUpdateException`; return typed `Error` codes for not-found/no-op outcomes and let unexpected exceptions propagate.
- In Managers, forward DataStore `Result` values unchanged unless adding boundary validation or input normalization that belongs above persistence.

## Examples
- `var ok = Result.Success();`
- `var saved = Result.Success(user);`
- `var failed = Result.Failure<User>(new Error("users.save.failed", "Unable to save the user.", ex));`
- `Result<Guid> id = Guid.NewGuid();`

## Anti-Patterns
- Do not expose public constructors for `Result` or `Result<T>`.
- Do not use sentinel values (`null`, `false`, empty collections) for expected failures once a Result-based path exists.
- Do not strip exception context when an `Error` can carry the underlying exception reference.
