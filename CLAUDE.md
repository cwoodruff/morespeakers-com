# CLAUDE.md

This file provides high-level, evergreen guidance for working with this repository.

## Top 5 Project Constraints

1.  **Tech Stack**: .NET 10, ASP.NET Core, Razor Pages. Frontend interactivity is done with **HTMX and Hyperscript**. No SPAs (React, Angular, Vue).
2.  **Database Workflow**: **NO Entity Framework Migrations**. All schema changes are done via raw SQL scripts in `scripts/database/`.
3.  **Orchestration**: Development is run via .NET Aspire. The AppHost (`src/MoreSpeakers.AppHost/AppHost.cs`) orchestrates the SQL Server container and loads the database scripts.
4.  **Architecture**: Follows a clean, vertical slice pattern: Web -> Managers -> Data -> Domain. Business logic lives in Managers.
5.  **Testing**: All unit tests are written with xUnit, FluentAssertions, and Moq.

## How to Run Locally

```bash
# From the /src directory
dotnet run --project MoreSpeakers.AppHost
```

## AI Skills & Commands

This project uses a hybrid system of AI Skills and Slash Commands for development.

-   **Skills** provide detailed, contextual instructions that Claude can use automatically.
-   **Slash Commands** are explicit triggers for common workflows.

See `docs/ai-skills.md` for a complete guide.

### Key Skills
-   `@dotnet-feature`: For implementing full-stack features.
-   `@sql-schema`: For managing database schema changes.
-   `@qa-engineer`: For writing unit tests.

### Key Commands
-   `/feature <name>`: Start a new vertical slice feature.
-   `/db-change <description>`: Guide a database schema modification.
-   `/test <target>`: Generate tests for a class or feature.
-   `/docs <topic>`: Create or update documentation.
