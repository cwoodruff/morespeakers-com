---
name: "xunit-v3-discovery"
description: "Fix xUnit v3 test projects that compile but show zero discovered tests under dotnet test."
domain: "testing"
confidence: "high"
source: "earned"
---

## Context
Use this when a .NET test project builds cleanly, but `dotnet test` reports that no tests are available.

## Pattern
- Check whether the project uses xUnit v3 packages without a Visual Studio/VSTest runner package.
- If `dotnet test` is the expected execution path, add `xunit.runner.visualstudio` to the test project.
- Re-run the targeted test project immediately to confirm tests are actually discovered and executed.

## Example
- `src\MoreSpeakers.Data.Tests\MoreSpeakers.Data.Tests.csproj` needed `xunit.runner.visualstudio` before `ExpertiseDataStoreResultTests` would run.

## Anti-Patterns
- Do not assume green build output means tests ran.
- Do not add new test projects when the existing one only needs runner configuration.
