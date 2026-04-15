# Squad Decisions

## Active Decisions

- **UI architecture:** Keep the user experience server-rendered with Razor Pages and HTMX.
- **Test stack:** Use xUnit, FluentAssertions, Moq, and Bogus for automated tests.
- **Deployment:** Publish production through GitHub Actions to Azure.
- **Exception handling:** Adopt Result<T> pattern for expected failures (approved 2025-07-18, backlog created 2026-04-14).

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

**Status:** Approved + Backlog Created (2025-07-18, finalized 2026-04-14) | **Severity:** High | **Owner:** Ripley | **Tracks:** #385–#394

**Decision:** Use `Result<T>` for expected failures. Reserve exceptions for truly exceptional conditions.

**Audit findings:** 74 catch blocks (all bare `catch(Exception)`), three conflicting patterns:
1. Swallow-and-return-sentinel (dominant) — DataStores return `bool`/`null`; caller gets no context
2. Catch-log-throw-ApplicationException (SaveAsync methods) — discards stack trace, uses generic message
3. Catch-everything-at-Web-layer (~29 handlers) — generic UI responses, no structured error

**Why Result<T> over rethrow-with-context:**
- Rethrow-with-context fixes only pattern #2
- Result<T> handles all three uniformly, making failure explicit and typed
- IdentityResult already proves pattern works in this codebase
- Structured error carries meaningful context through layers: DataStore → Manager → Web

**Implementation (incremental):**
1. **Phase 1 — Foundation (#385):** Add `Result<T>` and `Result` structs to Domain
2. **Phase 2 — PoC (#386):** Convert Expertise (one vertical slice)
3. **Phase 3 — Extend (#387–#389):** Remaining verticals (User, Mentoring, Sector+SocialMediaSite)
4. **Phase 4 — Cleanup (#390):** Web Services, remaining catch blocks, ApplicationException cleanup

**Sequencing:** #385 → #386 (PoC) → #387–#389 (parallel) → #390 (cleanup) | #391–#393 (independent, ship immediately) | #394 (grows with each vertical)

**Risks:** Large surface area (69 catch blocks); mitigated by incremental rollout and coexistence with sentinel pattern.

**Status:** Backlog issues created (10 total: #385–#394). Implementation begins with #385 foundation.

### Code Cleanup: Console.WriteLine

**Status:** Pending | **Severity:** Medium | **Owner:** api-agent

`OpenGraphSpeakerProfileImageGenerator.cs:80` uses `Console.WriteLine` instead of injected logger.

### Testing: DataStore Layer Coverage

**Status:** Pending (blocked on exception handling decision) | **Severity:** High | **Owner:** test-agent

Zero test coverage on Data layer. Requires test harness design after exception pattern is finalized.

### Project Documentation: README Reconciliation

**Status:** Applied (2026-07-16) | **Severity:** Low

Updated README.md to reflect .NET 10, EF Core 10 (runtime ORM), actual project structure, and no EF migrations constraint.

### Squad Routing: Result<T> Backlog (#385–#394)

**Status:** Applied (2026-04-15) | **Owner:** Ripley

**Decision:** Assign 10 backlog issues to squad members per expertise and architecture ownership:

| Group | Issue | Squad | Rationale |
|-------|-------|-------|-----------|
| Phase 1 | #385 | Dallas | Foundation: Domain Result<T> types. |
| Phase 2 | #386 | Dallas | Expertise PoC (full vertical). |
| Phase 2 | #391 | Bishop | XSS: Html.Raw Razor (independent). |
| Phase 2 | #392 | Bishop | XSS: innerHTML JS (independent). |
| Phase 2 | #393 | Dallas | Console.WriteLine cleanup (independent). |
| Phase 3 | #387, #388, #389 | Dallas | User, Mentoring, Sector+SocialMediaSite verticals (parallel after PoC). |
| Phase 3 | #394 | Vasquez | Test harness (incremental, grows with each vertical). |
| Phase 4 | #390 | Dallas | Cross-cutting cleanup (after verticals). |

**Labels:** `squad:dallas`, `squad:bishop`, `squad:vasquez` applied to all issues.  
**Result:** Squad members unblocked to begin work.

## Work Directives (2026-04-15)

### User Directive: Continue Exception Standardization

**Status:** Active | **Owner:** Squad  
**What:** Continue standardizing exception handling as an active team decision.  
**Why:** User request — aligns with Result<T> adoption across backlog (#385–#394).

### User Directive: Git Branch Naming

**Status:** Active | **Owner:** Squad  
**What:** For issue work, use git branches named as `issue-number-brief-description`.  
**Why:** User request — improves traceability and branch hygiene.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
