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

## Learnings

- The application UX is built with Razor Pages and HTMX, not a SPA.
- Expertise admin pages should send redirect failures through `TempData["ErrorMessage"]` so the shared admin toast layout can render feedback after redirects.
- The Register/Profile new-expertise HTMX partials must reuse `Model.NewExpertiseResponse` or server-side failure messages disappear on re-render.
- Key Web-layer touchpoints for Expertise Result handling are `src\MoreSpeakers.Web\Endpoints\ExpertiseEndpoints.cs`, `Pages\Speakers\Index.cshtml.cs`, `Areas\Identity\Pages\Account\Register.cshtml.cs`, and `Pages\Profile\Edit.cshtml.cs`.
