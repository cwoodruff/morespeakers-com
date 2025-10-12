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

- **Backend**: ASP.NET Core 8.0 with Razor Pages
- **Frontend**: HTML5, CSS3, HTMX, Hyperscript (minimal JavaScript)
- **Database**: Microsoft SQL Server (Local for development, Azure SQL for production)
- **Development Environment**: .NET Aspire for orchestration
- **Hosting**: Azure App Service
- **Database Hosting**: Azure SQL Database

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022, JetBrains Rider, or VS Code
- Docker Desktop (for .NET Aspire)
- Azure Storage Explorer

### Installation

1. Clone the repository:

```bash
git clone https://github.com/cwoodruff/morespeakers.com.git
cd morespeakers.com
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Run the application with Aspire:

```bash
dotnet run --project MoreSpeakers.AppHost
```

## Project Structure

```
MoreSpeakers/
| src/                                # Source code
│   ├── MoreSpeakers.AppHost/         # .NET Aspire orchestration
│   ├── MoreSpeakers.ServiceDefaults/ # .NET Aspire orchestration
│   ├── MoreSpeakers.Domain/          # Domain models
│   ├── MoreSpeakers.Functions/       # Azure Functions to support the application, like email notifications
│   ├── MoreSpeakers.Managers/        # Appplication services/managers/business logic
│   ├── MoreSpeakers.Tests/           # Unit tests
│   └── MoreSpeakers.Web/             # Main web application
│       ├── Pages/                    # Razor Pages
│       ├── Models/                   # Data models
│       ├── Data/                     # Entity Framework context
│       ├── wwwroot/                  # Static files (CSS, JS, images)
│       └── Areas/                    # Feature areas
├── docs/                             # Documentation
├── scripts/database                  # Scripts for the database deployment
└── .github/                          # Deployment files
```

## Development

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

See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for detailed deployment instructions.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support, please create an issue in the GitHub repository or contact the maintainers.

## Acknowledgments

- Thanks to the speaking community for inspiration
- HTMX and Hyperscript for making frontend development enjoyable
- .NET Aspire team for excellent developer experience tools
