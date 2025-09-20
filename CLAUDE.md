# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Running the Application
```bash
# Run with .NET Aspire (recommended for development)
dotnet run --project morespeakers.AppHost

# Run the web application directly
dotnet run --project morespeakers
```

### Building and Testing
```bash
# Build the entire solution
dotnet build

# Build specific project
dotnet build morespeakers/morespeakers.csproj

# Test (when tests are added)
dotnet test

# Restore dependencies
dotnet restore
```

### Database Operations
```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project morespeakers

# Apply migrations to database
dotnet ef database update --project morespeakers

# Drop database (development only)
dotnet ef database drop --project morespeakers
```

## Architecture Overview

This is a **mentorship platform** built with **.NET 9** and **ASP.NET Core** using **Razor Pages**. The application connects new speakers with experienced mentors in the technology community.

### Project Structure
- **morespeakers/** - Main web application (Razor Pages, Entity Framework, Identity)
- **morespeakers.AppHost/** - .NET Aspire orchestration host
- **morespeakers.ServiceDefaults/** - Shared service configuration
- **docs/** - Project documentation
- **tests/** - Test projects (currently empty)

### Key Technologies
- **.NET 9** with Aspire for orchestration
- **ASP.NET Core Identity** for authentication/authorization
- **Entity Framework Core** with SQL Server
- **HTMX + Hyperscript** for minimal JavaScript frontend
- **Redis** for distributed caching

### Data Model
Core entities:
- **User** - Extends Identity user with speaker profile data
- **SpeakerType** - NewSpeaker vs ExperiencedSpeaker
- **Expertise** - Technology/topic expertise areas
- **UserExpertise** - Many-to-many relationship between users and expertise
- **Mentorship** - Tracks mentor-mentee relationships
- **SocialMedia** - User's social media links

### Service Layer
Key services in `morespeakers/Services/`:
- **ISpeakerService** - Speaker profile management
- **IMentorshipService** - Mentorship relationship logic
- **IExpertiseService** - Expertise management
- **IFileUploadService** - File upload handling

### Database Context
`ApplicationDbContext` includes:
- Entity configurations with indexes and constraints
- Automatic timestamp updates
- Seeded data for speaker types, expertise areas, and Identity roles
- GUID primary keys with SQL Server NEWID() defaults

### Frontend Architecture
- **Server-side rendering** with Razor Pages
- **HTMX** for dynamic updates without full page reloads
- **Hyperscript** for declarative client-side interactions
- **Bootstrap** for responsive UI components
- Minimal JavaScript footprint

### Authentication & Authorization
- ASP.NET Core Identity with custom User model
- Role-based authorization (NewSpeaker, ExperiencedSpeaker, Administrator)
- Custom password policies and lockout settings

## Development Notes

### Running Locally
The application uses .NET Aspire for orchestration, which automatically manages:
- SQL Server database container
- Redis cache container
- Application dependencies and service discovery

### Database
- Uses SQL Server with Entity Framework Code First
- Includes automatic migrations on startup
- SQLite fallback for development (app.db file)
- Seeded with common expertise areas and default roles

### Key Patterns
- **Page Model pattern** for Razor Pages
- **Service layer** for business logic
- **Repository pattern** (optional, can be added as needed)
- **Dependency injection** throughout

### File Structure Conventions
- Pages follow feature-based organization
- Models separated into entities, view models, and DTOs
- Services use interface-based dependency injection
- Data configurations kept in ApplicationDbContext