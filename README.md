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
- Create mentor profiles highlighting expertise and available suppo~~~~rt
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

- **Backend**: ASP.NET Core 9.0 with Razor Pages
- **Frontend**: HTML5, CSS3, HTMX, Hyperscript (minimal JavaScript), Bootstrap 5
- **ORM**: Entity Framework Core 9.0
- **Database**: Microsoft SQL Server (Local for development, Azure SQL for production)
- **Development Environment**: .NET Aspire for orchestration
- **Hosting**: Azure App Service
- **Database Hosting**: Azure SQL Database
- **Email Notifications**: Azure Functions with SendGrid
- **Version Control**: Git and GitHub
- **CI/CD**: GitHub Actions
- **Containerization**: Docker (for local development with .NET Aspire)
- **Testing**: xUnit, Moq
- **Logging**: Serilog
- **Documentation**: Markdown files in the docs/ directory
- **Code Quality**: SonarCloud
- **Monitoring**: Azure Application Insights
- **Security**: ASP.NET Core Identity, Azure Key Vault
- **Caching**: In-memory caching with ASP.NET Core MemoryCache
- **Search**: Azure Cognitive Search (planned for future releases)

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022, JetBrains Rider, or VS Code
- Docker Desktop (for .NET Aspire)
- Azure Storage Explorer

### Developer Setup

Read about set up and developer solution installation in [Developer Startup](docs/developer-startup.md).

## Project Structure

```
MoreSpeakers/
| src/                                # Source code
│   ├── MoreSpeakers.AppHost/         # .NET Aspire orchestration
│   ├── MoreSpeakers.ServiceDefaults/ # .NET Aspire service defaults
│   ├── MoreSpeakers.Domain/          # Domain models and abstractions
│   ├── Morespeakers.Functions/       # Azure Functions (e.g., email notifications)
│   ├── MoreSpeakers.Managers/        # Application services/managers/business logic
│   ├── MoreSpeakers.Tests/           # Automated tests
│   └── MoreSpeakers.Web/             # Main web application
│       ├── Areas/                    # Feature areas (e.g., Identity)
│       ├── Data/                     # EF Core DbContext and Migrations
│       ├── Models/                   # View models and data models
│       ├── Pages/                    # Razor Pages
│       ├── Services/                 # App-level services used by the web app
│       ├── wwwroot/                  # Static files (CSS, JS, images, libs)
│       └── Properties/               # Launch settings and config
├── docs/                             # Documentation
├── scripts/database/                 # Database deployment scripts
└── .github/                          # CI/CD and workflows
```

### Database Migrations
Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

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

See [architecture_overview.md](docs/architecture_overview.md) for a detailed explanation of the system architecture, design decisions, and patterns used in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support, please create an issue in the GitHub repository or contact the maintainers.

## Acknowledgments

- Thanks to the speaking community for inspiration
- HTMX and Hyperscript for making frontend development enjoyable
- .NET Aspire team for excellent developer experience tools
