# Project Context

- **Project:** morespeakers-com
- **Owner:** Joseph Guadagno
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, HTMX, Azure Functions, xUnit, FluentAssertions, Moq, Bogus, GitHub Actions, Azure

## Core Context

- Owns CI/CD and Azure delivery for the MoreSpeakers squad.
- Focus on GitHub Actions and production deployment safety.

## Recent Updates

- 2026-04-13: Team initialized with the Alien cast.

## Learnings

- Production is published via GitHub Actions to Azure.
- 2026-05-22: Created PR #403 to add CI workflow for pull request testing (issue #383):
  - Solution uses .NET 10.0.x SDK (net10.0 target framework)
  - Four test projects exist: Data.Tests, Domain.Tests, Managers.Tests, Web.Tests
  - Workflow pattern: checkout → setup .NET → restore → build (Release, --no-restore) → test (--no-build)
  - Used dorny/test-reporter@v1 for publishing TRX test results with PR visibility
  - Existing workflows use DOTNET_CORE_VERSION: 10.0.x env var for consistency
  - Aspire AppHost deliberately excluded from test runs (avoid orchestration overhead in CI)
- 2026-05-22: Fixed dorny/test-reporter failure in CI workflow (PR #403, branch issue-383-ci-pr-tests):
  - Root cause: dorny/test-reporter@v1 requires `checks: write` permission to publish test results to PRs
  - Fix: Added `checks: write` to job permissions block (alongside existing `contents: read` and `pull-requests: write`)
  - This is a GitHub Actions runner permission requirement, not a token or action availability issue
  - Test reporter now has the necessary permissions to create check runs on pull requests
- 2026-05-22: Resolved merge conflicts for PR #403 (branch issue-383-ci-pr-tests vs main):
  - Conflict in `.github/workflows/ci-pr.yml` due to both branches adding the file independently
  - main branch version lacked the critical `checks: write` permission
  - Resolution: Preserved Parker's complete workflow including `checks: write` (required for test-reporter)
  - Also brought in XSS fixes from main: Html.Raw removals in Profile/_ProfileEditForm and _PasswordChangeForm
  - Merge strategy: Keep Parker's CI workflow intact while adopting main's security improvements
  - Result: PR #403 no longer has conflicts and maintains all necessary permissions for test reporting
