# Squad Decisions

## Active Decisions

- **UI architecture:** Keep the user experience server-rendered with Razor Pages and HTMX.
- **Test stack:** Use xUnit, FluentAssertions, Moq, and Bogus for automated tests.
- **Deployment:** Publish production through GitHub Actions to Azure.

## Codebase Quality & Architecture (2026-04-13)

### Security: XSS via Html.Raw and JavaScript innerHTML

**Status:** Proposed | **Severity:** Critical | **Owner:** api-agent

Three Razor partials render validation messages unsafely:
- `Pages/Profile/_ProfileEditForm.cshtml` (lines 7, 13)
- `Pages/Profile/_PasswordChangeForm.cshtml` (line 6)

Multiple JS files use `innerHTML` with server responses:
- `register.js`, `expertise.js`, `name-validation-input.js`, `passkeys.js`

**Fix:** Replace Html.Raw with safe Razor encoding. Use textContent or DOM creation in JS.

### Exception Handling: Adopt Result<T> Pattern (2025-07-18)

**Status:** Proposed | **Severity:** High | **Owner:** Ripley | **Blockers:** Unblocks XSS fixes, DataStore tests

**Decision:** Use `Result<T>` for expected failures. Reserve exceptions for truly exceptional conditions.

**Audit findings:** 69 catch blocks (all bare `catch(Exception)`), three conflicting patterns:
1. Swallow-and-return-sentinel (dominant) — DataStores return `bool`/`null`; caller gets no context
2. Catch-log-throw-ApplicationException (SaveAsync methods) — discards stack trace, uses generic message
3. Catch-everything-at-Web-layer (~29 handlers) — generic UI responses, no structured error

**Why Result<T> over rethrow-with-context:**
- Rethrow-with-context fixes only pattern #2
- Result<T> handles all three uniformly, making failure explicit and typed
- IdentityResult already proves pattern works in this codebase
- Structured error carries meaningful context through layers: DataStore → Manager → Web

**Implementation (incremental):**
1. **Phase 1 — Foundation:** Add `Result<T>` and `Result` structs to Domain
2. **Phase 2 — PoC:** Convert Expertise (one vertical slice)
3. **Phase 3 — Extend:** Remaining verticals, one PR each
4. **Phase 4 — Cleanup:** Remove defensive catch blocks, replace ApplicationException throws

**Risks:** Large surface area (69 catch blocks); mitigated by incremental rollout and coexistence with sentinel pattern.

**Awaiting:** Team consensus to proceed with Phase 1.

### Code Cleanup: Console.WriteLine

**Status:** Pending | **Severity:** Medium | **Owner:** api-agent

`OpenGraphSpeakerProfileImageGenerator.cs:80` uses `Console.WriteLine` instead of injected logger.

### Testing: DataStore Layer Coverage

**Status:** Pending (blocked on exception handling decision) | **Severity:** High | **Owner:** test-agent

Zero test coverage on Data layer. Requires test harness design after exception pattern is finalized.

### Project Documentation: README Reconciliation

**Status:** Applied (2026-07-16) | **Severity:** Low

Updated README.md to reflect .NET 10, EF Core 10 (runtime ORM), actual project structure, and no EF migrations constraint.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
