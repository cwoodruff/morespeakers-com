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

### Exception Handling: Adopt Unified Pattern

**Status:** Pending Team Decision | **Severity:** High | **Blockers:** Unblocks XSS fixes, DataStore tests

35+ `catch (Exception)` blocks with inconsistent handling. Team must choose:
- `Result<T>` pattern, or
- Standardized rethrow-with-context

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
