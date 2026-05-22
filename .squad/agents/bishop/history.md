# Project Context

- **Project:** morespeakers-com
- **Owner:** Joseph Guadagno
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, HTMX, Azure Functions, xUnit, FluentAssertions, Moq, Bogus, GitHub Actions, Azure

## Core Context

- Owns Razor Pages and HTMX work for the MoreSpeakers squad.
- Focus on server-rendered UX and page behavior.

## Recent Updates

- 2026-04-13: Team initialized with the Alien cast.
- 2026-04-15: Issue #386 Expertise Result<T> web surface completed. Admin Expertise pages use inline `ModelState` for same-page errors and `TempData["ErrorMessage"]` for redirects. HTMX new-expertise in Register/Profile now render `Model.NewExpertiseResponse` to preserve server-rendered failure feedback. Fixed partial re-render bug that dropped validation errors. Speakers filters and homepage popular-expertise updated for Result surface. All web tests pass (48/48). Orchestration log archived. Web Result patterns now established for downstream verticals.
- 2026-05-22: **Security milestone complete.** Issue #391 (PR #400): Fixed XSS via Html.Raw in `_ProfileEditForm.cshtml` and `_PasswordChangeForm.cshtml`. Issue #392 (PR #401): Fixed XSS via innerHTML in `expertise.js`, `register.js`, `name-validation-input.js`, `passkeys.js`, `file-upload.js`. Pattern: All server data rendered via createElement+textContent; static innerHTML annotated. Both PRs independent, no blockers. Decisions codified in squad ledger.

## Learnings

- Issue #392 (2026-04-15): For icon+text messages injected from server responses, the safe pattern is: create the `<i>` element with `document.createElement`, set its className, clear the container with `textContent = ''`, then append the icon and a `document.createTextNode(message)`. Never interpolate server data into innerHTML.
- Static-only innerHTML (spinners, loading indicators, page headers with no server data) should be annotated `// Safe: static string, no user data` rather than refactored, to signal intentional review.
- `file-upload.js` file preview used `file.name` (client filesystem name) inside a template literal — still needs `textContent` since user-controlled input should never be injected via innerHTML.
- `passkeys.js` `err.message` comes from caught JavaScript errors which can be influenced by server responses; always treat as untrusted and use `textContent`.

- Issue #391 (2026-04-15): Fixed XSS via Html.Raw in profile partials. Three `@Html.Raw(...)` calls in `_ProfileEditForm.cshtml` (lines 7, 13) and `_PasswordChangeForm.cshtml` (line 6) replaced with plain `@Model.ValidationMessage` / `@Model.SuccessMessage`. Razor's default encoding is sufficient — static Bootstrap Icon markup (`<i class="bi bi-...">`) is hardcoded and safe.
- The application UX is built with Razor Pages and HTMX, not a SPA.
- Expertise admin pages should send redirect failures through `TempData["ErrorMessage"]` so the shared admin toast layout can render feedback after redirects.
- The Register/Profile new-expertise HTMX partials must reuse `Model.NewExpertiseResponse` or server-side failure messages disappear on re-render.
- Key Web-layer touchpoints for Expertise Result handling are `src\MoreSpeakers.Web\Endpoints\ExpertiseEndpoints.cs`, `Pages\Speakers\Index.cshtml.cs`, `Areas\Identity\Pages\Account\Register.cshtml.cs`, and `Pages\Profile\Edit.cshtml.cs`.
