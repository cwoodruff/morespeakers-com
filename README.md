# MoreSpeakers.com

A web application connecting new speakers with experienced mentors in the technology and software development community.

## Overview

MoreSpeakers.com is a mentorship platform designed to help aspiring speakers in the technology industry connect with experienced conference and community speakers. The platform facilitates knowledge sharing, guidance, and support for individuals looking to improve their speaking skills and break into the conference speaking circuit.

## Features

### For New Speakers
- Create detailed profiles with expertise areas and speaking goals
- Browse experienced speakers and request mentorship
- Receive guidance on talk creation, conference selection, and speaking skills
- Track mentorship relationships

### For Experienced Speakers
- Create mentor profiles highlighting expertise and available support
- Browse new speakers seeking mentorship
- Offer guidance and assign yourself to new speakers
- Manage multiple mentorship relationships

### Core Functionality
- User registration and authentication for both new and experienced speakers
- Dynamic profile creation with social media integration
- Tag-based expertise matching system
- Mentorship assignment and management
- Responsive, modern web interface

## Technology Stack

- **Backend**: .NET 10 / ASP.NET Core with Razor Pages
- **Frontend**: HTML5, CSS3, HTMX, Hyperscript (minimal JavaScript), Bootstrap 5
- **Data Access**: Entity Framework Core 10 (runtime ORM only — **no EF migrations**)
- **Database Schema**: Raw SQL scripts in `scripts/database/`, loaded by .NET Aspire at startup
- **Database**: Microsoft SQL Server (containerized via Docker for development, Azure SQL for production)
- **Development Environment**: .NET Aspire for orchestration (AppHost manages SQL Server, Azure Storage emulator)
- **Hosting**: Azure App Service
- **Database Hosting**: Azure SQL Database
- **Email Notifications**: Azure Functions with SendGrid
- **Version Control**: Git and GitHub
- **CI/CD**: GitHub Actions
- **Containerization**: Docker (for local development with .NET Aspire)
- **Testing**: xUnit, FluentAssertions, Moq, Bogus
- **Logging**: Serilog
- **Object Mapping**: AutoMapper (Data ↔ Domain model mapping)
- **Documentation**: Markdown files in the docs/ directory
- **Code Quality**: SonarCloud
- **Monitoring**: Azure Application Insights
- **Security**: ASP.NET Core Identity (with passkey support), Azure Key Vault
- **Caching**: In-memory caching with ASP.NET Core MemoryCache

## Quick Start

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022, JetBrains Rider, or VS Code
- Docker Desktop (for .NET Aspire)
- Azure Storage Explorer

### Developer Setup

Read about set up and developer solution installation in [Developer Startup](docs/developer-startup.md).

## Project Structure

```
MoreSpeakers/
├── src/                                  # Source code
│   ├── MoreSpeakers.AppHost/             # .NET Aspire orchestration (entry point for dev)
│   ├── MoreSpeakers.ServiceDefaults/     # .NET Aspire shared service configuration
│   ├── MoreSpeakers.Domain/              # Domain models, interfaces, and validation
│   ├── MoreSpeakers.Data/                # EF Core DbContext, DataStores, AutoMapper profiles
│   ├── MoreSpeakers.Managers/            # Business logic / application services
│   ├── MoreSpeakers.Functions/           # Azure Functions (email, OpenGraph image generation)
│   ├── MoreSpeakers.Web/                 # Main Razor Pages web application
│   │   ├── Areas/                        # Feature areas (Identity, Admin)
│   │   ├── Authorization/                # Policies and role constants
│   │   ├── Endpoints/                    # Minimal API endpoints (e.g., Expertise API)
│   │   ├── Models/                       # View models
│   │   ├── Pages/                        # Razor Pages
│   │   ├── Services/                     # App-level services (file upload, email, OpenGraph)
│   │   ├── TagHelpers/                   # Custom Razor tag helpers
│   │   └── wwwroot/                      # Static files (CSS, JS, images, libs)
│   ├── MoreSpeakers.Data.Tests/          # Data layer unit tests
│   ├── MoreSpeakers.Domain.Tests/        # Domain model unit tests
│   ├── MoreSpeakers.Managers.Tests/      # Manager/business logic unit tests
│   └── MoreSpeakers.Web.Tests/           # Web layer unit tests
├── docs/                                 # Documentation
├── scripts/database/                     # Raw SQL scripts (schema, views, functions, seed data)
└── .github/                              # CI/CD workflows
```

### Database Changes

This project does **not** use EF Core migrations. Schema changes are managed via raw SQL scripts in `scripts/database/`. The .NET Aspire AppHost loads and executes these scripts when the SQL Server container starts.

To make a schema change, add or modify the appropriate SQL script (e.g., `create-tables.sql`, `seed-data.sql`).

### Running Tests
```bash
dotnet test
```

### Code Style
This project follows standard C# coding conventions. Please ensure your IDE is configured with EditorConfig support.

## Deployment

### Azure Deployment

See [deployment_guide.md](docs/deployment_guide.md) for detailed deployment instructions.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Check out the [Developer Startup](docs/developer-startup.md) for detailed instructions on setting up your development environment to work on this project.

## Architecture Overview

See [architecture.md](docs/architecture.md) for a detailed explanation of the system architecture, design decisions, and patterns used in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support, please create an issue in the GitHub repository or contact the maintainers.

## Acknowledgments

- Thanks to the speaking community for inspiration
- HTMX and Hyperscript for making frontend development enjoyable
- .NET Aspire team for excellent developer experience tools
