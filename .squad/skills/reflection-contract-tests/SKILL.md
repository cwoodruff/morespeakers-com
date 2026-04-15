---
name: "reflection-contract-tests"
description: "Use reflection-based tests to lock an API contract before the implementation lands."
domain: "testing"
confidence: "high"
source: "earned"
---

## Context
Use this when a team wants tests merged ahead of an additive implementation. The goal is to keep the test project compiling while still failing loudly if required public types or members are missing.

## Patterns
- Load the target assembly from an existing known type instead of referencing the new contract directly.
- Assert on public type names, namespace, shape, factory methods, operators, and property values through reflection.
- Fail with explicit messages that name the missing type or member so the implementer gets a clean failure story.

## Examples
- `src/MoreSpeakers.Domain.Tests\ResultFoundationTests.cs` validates the future `MoreSpeakers.Domain.Result`, `MoreSpeakers.Domain.Result<T>`, and `MoreSpeakers.Domain.Error` contract before those types exist.

## Anti-Patterns
- Do not use reflection when the implementation already exists and compile-time assertions are simpler.
- Do not make reflection tests so vague that multiple incompatible APIs would pass.
