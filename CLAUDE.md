# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Running the Application
```bash
# Run with .NET Aspire (recommended for development)
# Note: Working directory is /src
dotnet run --project MoreSpeakers.AppHost

# Run the web application directly
dotnet run --project MoreSpeakers.Web
```

### Building and Testing
```bash
# Build the entire solution
dotnet build MoreSpeakers.sln

# Build specific project
dotnet build MoreSpeakers.Web/MoreSpeakers.Web.csproj

# Run all tests (xUnit with FluentAssertions, Moq, Bogus)
dotnet test

# Run specific test class
dotnet test --filter "SpeakerServiceTests"

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests by category
dotnet test --filter "Category=Unit"

# Restore dependencies
dotnet restore
```

### Database Operations

Entity Framework migrations are not used in the application

## Architecture Overview

This is a **mentorship platform** built with **.NET 10** and **ASP.NET Core** using **Razor Pages**. The application connects new speakers with experienced mentors in the technology community.

### Project Structure
- **MoreSpeakers.Web/** - Main web application (Razor Pages, Entity Framework, Identity)
- **MoreSpeakers.AppHost/** - .NET Aspire orchestration host
- **MoreSpeakers.ServiceDefaults/** - Shared service configuration
- **MoreSpeakers.Tests/** - xUnit test suite with comprehensive coverage

**Note:** The working directory is `/src` - all project paths are relative to this directory.

### Key Technologies
- **.NET 10** with Aspire for orchestration
- **ASP.NET Core Identity** with a custom User model (GUID-based)
- **Entity Framework Core** with SQL Server
- **HTMX + Hyperscript** for minimal JavaScript frontend
- **Serilog** for structured logging with Application Insights integration
- **Bootstrap** for responsive UI

### Data Model
Core entities (all in `MoreSpeakers.Web/Models/`):
- **User** - Extends IdentityUser<Guid> with speaker profile data
- **SpeakerType** - NewSpeaker vs ExperiencedSpeaker
- **Expertise** - Technology/topic expertise areas (40+ seeded areas)
- **UserExpertise** - Many-to-many relationship between users and expertise
- **Mentorship** - Tracks mentor-mentee relationships with status and type enums
- **MentorshipExpertise** - Many-to-many relationship for mentorship focus areas
- **SocialMedia** - User's social media links

**Important:** Mentorships have a database constraint preventing self-mentoring (`CHK_Mentorships_DifferentUsers`)

### Service Layer
Key services in `MoreSpeakers.Web/Services/`:
- **ISpeakerService** - Speaker profile management
- **IMentorshipService** - Mentorship relationship logic
- **IExpertiseService** - Expertise management
- **IFileUploadService** - File upload handling

All services are registered as scoped in `Program.cs`

### Database Context
`ApplicationDbContext` (in `MoreSpeakers.Web/Data/`):
- Extends `IdentityDbContext<User, IdentityRole<Guid>, Guid>`
- Entity configurations with indexes and constraints
- Automatic timestamp updates via `SaveChanges()` override
- Seeded data: 2 speaker types, 40+ expertise areas, 3 Identity roles
- GUID primary keys with SQL Server NEWID() defaults

### .NET Aspire Configuration
`MoreSpeakers.AppHost/AppHost.cs`:
- Manages SQL Server 2022 container with persistent lifetime
- Executes database creation scripts from `/scripts/database/` in order:
  1. create-database.sql
  2. create-tables.sql
  3. create-views.sql
  4. create-functions.sql
  5. seed-data.sql
- Configures health check endpoint at `/health`
- Uses `WaitFor()` pattern to ensure database is ready before web app starts

### Frontend Architecture
- **Server-side rendering** with Razor Pages
- **HTMX** for dynamic updates without full page reloads
- **Hyperscript** for declarative client-side interactions
- **Bootstrap** for responsive UI components
- **libman** for client-side library management (libman.json in Web project)
- Minimal JavaScript footprint

### Authentication & Authorization
- ASP.NET Core Identity with custom User model (GUID-based IDs)
- Role-based authorization (NewSpeaker, ExperiencedSpeaker, Administrator)
- Password requirements: 8+ chars, digit, lowercase, uppercase, 1 unique char
- Lockout: 5 failed attempts = 5 minute lockout
- Authorization configured in `Program.cs` for Identity areas

### Logging and Telemetry
- **Serilog** with enrichers (machine name, thread ID, environment, assembly info)
- Console and file logging (rolling daily files in `logs/logs.txt`)
- **Application Insights** integration with Azure telemetry
- Custom telemetry initializer for Azure Web App role environment

### Testing Architecture
Test project uses:
- **xUnit** as the testing framework
- **FluentAssertions** for readable assertions
- **Moq** for mocking dependencies
- **Microsoft.EntityFrameworkCore.InMemory** for database testing
- **Bogus** for fake data generation
- **Microsoft.AspNetCore.Mvc.Testing** for integration tests
- Base class `TestBase.cs` provides in-memory database setup

## Development Notes

### Running Locally
The application uses .NET Aspire for orchestration, which automatically manages:
- SQL Server 2022 database container (persistent lifetime)
- Application dependencies and service discovery
- Health check monitoring at `/health`

### Database
- Uses SQL Server 2022 via Aspire container
- Database initialization via SQL scripts (not EF migrations in Aspire mode)
- ApplicationDbContext handles Entity Framework Code First models
- Seeded with 40+ expertise areas (C#, .NET, Azure, JavaScript, Python, etc.) and 3 default roles

### Session Management
- Session timeout: 30 minutes
- HttpOnly and Essential cookies enabled
- Cookie consent checking enabled

### Key Patterns
- **Page Model pattern** for Razor Pages
- **Service layer** for business logic with DI via interfaces
- **Identity pattern** for authentication/authorization
- **Dependency injection** throughout via `IServiceCollection`
- **Timestamp tracking** via ApplicationDbContext SaveChanges override