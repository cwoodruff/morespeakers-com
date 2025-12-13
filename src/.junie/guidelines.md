# Project Guidelines

## Project-specific engineering guidelines for MoreSpeakers

These guidelines are tailored to the current solution structure, tech stack, and conventions observed in the repository. They are meant to be practical and lightweight. Save this to `docs/engineering-guidelines.md` if you want it versioned.

### Solution architecture and layout
- Solution: multi-project monorepo under `src/`
    - `MoreSpeakers.Web`: ASP.NET Core 10 Razor Pages UI (htmx/hyperscript present under `wwwroot/lib` via LibMan)
    - `MoreSpeakers.Domain`: domain models and interfaces (e.g., `IExpertiseManager`, `IUserManager`)
    - `MoreSpeakers.Managers`: application/services layer (e.g., `ExpertiseManager`)
    - `MoreSpeakers.Data`: data access layer (data stores, EF or custom persistence abstractions)
    - `MoreSpeakers.Functions`: background jobs/Azure Functions (if used)
    - Tests: `*.Tests` per layer (e.g., `MoreSpeakers.Managers.Tests` with FluentAssertions + Moq)
    - Shared settings: `.editorconfig` at `src/.editorconfig`

Design principles
- Keep Razor Pages lean; push business rules to `Managers` behind `Domain` interfaces.
- Constructors accept interfaces only; register concrete types in `Program.cs` (e.g., `builder.Services.AddScoped<IExpertiseManager, ExpertiseManager>();`).
- Domain models remain persistence-agnostic; no direct EF/HTTP/IO code in Domain.
- Async-first across IO boundaries; do not block on async (`.Result`, `.Wait()` are forbidden).

### Coding style (C#) — align with `.editorconfig`
- Indentation/spaces
    - 4 spaces; no tabs. `insert_final_newline = false` (keep as-is until project opts to change).
- Usings
    - Group and sort; `System` usings first; separate groups enabled.
- `var` usage
    - Prefer explicit types (all `csharp_style_var_* = false`).
- Expression-bodied members
    - Allowed for accessors, properties, indexers; avoid for methods/constructors unless clearly beneficial.
- Fields
    - Prefer `readonly` when possible (`dotnet_style_readonly_field = true:warning`).
- Local/static
    - Prefer static local/anonymous functions when feasible (`csharp_prefer_static_local_function = true:warning`).
- Patterns and nulls
    - Prefer pattern matching, switch expressions, null-propagation, and simplified interpolations when it aids clarity.
- File/Type layout
    - One public type per file; keep files under namespaces matching folder structure (`dotnet_style_namespace_match_folder = true`).

#### Naming and organization
- Projects/namespaces: `MoreSpeakers.{Layer}`. Types in `Namespaces` that map to folders.
- Interfaces: `I*` prefix (e.g., `IExpertiseManager`). Implementation mirrors name without `I` (e.g., `ExpertiseManager`).
- Razor Pages
    - Page models as `*.cshtml.cs`, class name `PageNameModel`. Keep handler methods short and orchestrative.
    - Bind properties explicitly; favor `SupportsGet = true` only when needed.

### Razor Pages patterns
- Handlers
    - Use `Task<IActionResult>` for `OnGet*/OnPost*` handlers.
    - Validate query/body before service calls; short-circuit invalid state with appropriate results (`BadRequest`, `Page()`, redirects).
- Pagination
    - Encapsulate constants (e.g., `PageSize = 12`) within page model or a shared options class.
- Partial rendering and emails
    - Use `IRazorPartialToStringRenderer` for server-side rendering of HTML partials.
    - Use `ITemplatedEmailSender` for email flows; avoid duplicating templates/string-concatenation in page models.
- HTMX/hyperscript
    - Prefer progressive enhancement: pages work without JS; HTMX augments interactions (partial updates, swaps).
    - Return partials for HTMX requests; use `hx-request` checks or routes that specifically return partials.

### Dependency Injection
- Register abstractions in `Program.cs`.
- Lifetime guidance
    - `Scoped` for request-bound services (most managers).
    - `Singleton` only for stateless, thread-safe services; avoid holding scoped dependencies.
    - `Transient` for lightweight, stateless services with no internal caching.
- Options/config
    - Use `IOptions<T>` for configuration; validate options at startup where critical.

### Error handling and logging
- Logging
    - Use `ILogger<T>` via DI; prefer structured logs: `_logger.LogInformation("Searching speakers with {Term} {Type} {Expertise}", SearchTerm, SpeakerTypeFilter, ExpertiseFilter);`
    - Pick log levels deliberately: `Trace` noisy internals; `Debug` for diagnostics; `Information` for business milestones; `Warning` for recoverable anomalies; `Error` for failures.
    - Do not log sensitive data (PII, tokens). Redact when necessary.
- Exceptions
    - Throw domain-specific exceptions in managers where it clarifies intent; catch at edges (PageModel) to present user-friendly messages.
    - Never swallow exceptions; at minimum, log with context.

### Validation and security
- Model validation
    - Use data annotations on input models/ViewModels. In handlers: check `ModelState.IsValid` before calling services.
- Anti-forgery
    - Razor Pages enable anti-forgery by default for POST; keep it enabled. Add `[ValidateAntiForgeryToken]` for custom endpoints if needed.
- Authorization
    - Use `[Authorize]` at page or folder level for protected areas (e.g., Profile/Edit). Use policies/roles claims-based where applicable.
- Input sanitization and encoding
    - Rely on Razor’s automatic HTML encoding. For any raw HTML, use safe lists and sanitizer.
- Secrets
    - Store secrets in environment variables or User Secrets in dev; never commit secrets.

### Data and managers
- Managers (application services)
    - Keep IO and business orchestration here. Expose async methods (`Task<T>`). Handle cross-entity logic.
- Data access
    - Keep repositories/data stores isolated in `MoreSpeakers.Data`. No direct data access from Web.
- Caching
    - Read-most data (e.g., expertise list for dropdowns) may be cached with expiration. Use `IMemoryCache` or `IDistributedCache` via DI.

### Frontend (wwwroot)
- Libraries via LibMan
    - Restore with `libman-restore.ps1` or LibMan.json (keep versions pinned). Do not manually edit library files under `wwwroot/lib`.
- JS organization
    - Place page-specific scripts under `wwwroot/js/` (e.g., `expertise.js`). Keep scripts idempotent and unobtrusive; prefer `data-*` attributes and small event handlers.
    - Favor HTMX for partial updates; avoid re-implementing AJAX flows.
- CSS/assets
    - Leverage Bootstrap/Bootswatch present under `wwwroot/lib`. Keep site overrides scoped to component/page when feasible.

### Testing
- Frameworks in use
    - Unit tests with xUnit + FluentAssertions; mocking with Moq (observed `Mock<>`).
- Strategy
    - Tests live in `*.Tests` projects colocated per layer.
    - Unit-test managers and domain logic. Razor PageModels: test handler behavior by injecting fakes/mocks.
    - Use AAA style; deterministic tests only.
- Naming
    - File/class: `<TypeUnderTest>Tests`. Method: `MethodName_ShouldExpectedBehavior_WhenCondition`.
- Coverage focus
    - Critical flows: speaker search filters/sorting/pagination; expertise creation/validation; mentorship browsing.

### Performance
- Always use async IO; avoid sync-over-async.
- Paginate queries by default; avoid returning unbounded collections to UI.
- Defer heavy work to background processes/functions when request timing isn’t critical.

### Pull requests and branching
- Branching
    - `main` stays releasable. Feature branches: `feature/<short-slug>`. Fix branches: `fix/<short-slug>`.
- Commits
    - Use Conventional Commits: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`. Keep messages imperative and scoped.
- PR checklist
    - [ ] Linked issue and concise description
    - [ ] Tests updated/added and passing locally
    - [ ] No breaking API changes without migration notes
    - [ ] Logging added where useful; no secrets in logs
    - [ ] UI tested with and without JS (progressive enhancement)
    - [ ] Screenshots for UI changes when appropriate

### CI/CD (baseline expectations)
- Pipelines should run: `dotnet restore; dotnet build -c Release; dotnet test --no-build` across all `*.sln` projects.
- Optionally add `dotnet format --verify-no-changes` to enforce `.editorconfig` rules.
- Ensure LibMan restore runs prior to building Web if required by bundling.

### Documentation
- Keep user-facing docs and architecture notes in `docs/`.
- For notable choices, add short ADRs (`docs/adr/NNN-title.md`).

### Ready-to-use checklists
- New PageModel
    - [ ] Define bound properties explicitly; `SupportsGet` only when needed
    - [ ] Validate input; handle `ModelState` errors
    - [ ] Use managers via interfaces; no direct data access
    - [ ] Log key events with structured data
    - [ ] Return partials for HTMX requests when applicable
- New Manager method
    - [ ] Async `Task`-based API; cancellation token if long-running
    - [ ] Validate arguments; throw meaningful exceptions
    - [ ] Log at boundaries; no PII in logs
    - [ ] Keep pure business logic unit-testable
- Expertise feature changes (common hotspot)
    - [ ] Reuse `IExpertiseManager` APIs
    - [ ] Consider duplicate/similar-name checks (`FuzzySearch`/`DoesExpertiseWithNameExistsAsync`)
    - [ ] Cache read lists where appropriate

### Examples from repo (for alignment)
- DI registration: `MoreSpeakers.Web/Program.cs` registers `IExpertiseManager -> ExpertiseManager`.
- PageModel orchestration: `Pages/Speakers/Index.cshtml.cs` shows pattern for filtering, pagination, and data loading via managers.
- Tests: `MoreSpeakers.Managers.Tests/ExpertiseManagerTests.cs` uses FluentAssertions and Moq — mirror this style for new tests.
