# MoreSpeakers.com – Architecture Overview

## Application Architecture

MoreSpeakers.com follows a clean, layered architecture built on ASP.NET Core with Razor Pages, emphasizing server-side rendering and minimal client-side JavaScript.

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Razor Pages   │  │      HTMX       │  │ Hyperscript  │ │
│  │   (Server-side) │  │  (Client-side)  │  │ (Scripting)  │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Page Models   │  │    Services     │  │ Validators   │ │
│  │                 │  │                 │  │              │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                     Data Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │ Entity Framework│  │    Repositories │  │   Models     │ │
│  │    DbContext    │  │   (Optional)    │  │              │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                    Database Layer                           │
│           Microsoft SQL Server / Azure SQL                  │
└─────────────────────────────────────────────────────────────┘
```

## Technology Stack Deep Dive

### Backend Technologies

**ASP.NET Core 10.0 with Razor Pages**
- Server-side rendering for optimal performance and SEO
- Built-in dependency injection
- Integrated authentication and authorization
- Cross-platform compatibility

**Entity Framework Core**
- Code-first database approach
- Automatic migrations
- LINQ query capabilities
- Connection pooling and performance optimization

### Frontend Technologies

**HTMX**
- Enables AJAX requests with minimal JavaScript
- Progressive enhancement approach
- Server-driven UI updates
- WebSocket support for real-time features

**Hyperscript**
- Declarative client-side scripting
- Event-driven interactions
- Complements HTMX for complex UI behaviors

**Modern CSS**
- Bootstrap 5 for styling
- CSS Grid and Flexbox for layouts
- Minimal use Custom CSS properties for theming
- Responsive design with a mobile-first approach

### Development Environment

**.NET Aspire**
- Orchestrates multi-project solutions
- Provides observability and monitoring
- Simplifies service discovery
- Hot reload and debugging support

## Project Structure

### Main Application (`MoreSpeakers.Web`)

```
MoreSpeakers.Web/
├── EmailTemplates/           # Email templates used throughout the application
├── Endpoints/                # Endpoints used for the application
├── Pages/
│   ├── Speakers/             # Speaker management
│   ├── Mentorship/           # Mentorship features
│   ├── Profile/              # User profiles
│   └── Shared/               # Shared layouts and partials
├── Models/
│   ├── ViewModels/           # Page view models
│   └── *                     # Assorted models used in the web application
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/       # Entity configurations
│   └── Migrations/           # EF migrations
├── Services/
│   └── *                     # Assorted services used in the web application
├── Authorization/
│   ├── AppRoles.cs           # Roles used in the application for authorization
│   └── PolicyNames.cs        # Authorization policies
├── Areas/
│   ├── Admin/                # Administrative features
│   └── Identity/             # Administrative features
└── wwwroot/
    ├── css/                  # Stylesheets
    ├── js/                   # JavaScript files
    └── images/               # Static images
```

### Aspire Host (`MoreSpeakers.AppHost`)

```
MoreSpeakers.AppHost/
├── Program.cs                # Aspire orchestration setup
├── Properties/
└── appsettings.json          # Development configuration
```

## Key Design Patterns

### Model-View-PageModel (MVPM)
- Separation of concerns between UI and business logic
- Page models handle HTTP requests and coordinate with services
- Views focus purely on presentation

### Repository Pattern (`MoreSpeakers.Data`)
- Abstraction layer over Entity Framework
- Useful for complex queries and testing

### Service Layer Pattern (`MoreSpeakers.Managers`)
- Business logic encapsulation
- Reusable across different controllers/pages
- Dependency injection for loose coupling

## Security Architecture

### Authentication & Authorization
- ASP.NET Core Identity for user management
- Role-based authorization (NewSpeaker, ExperiencedSpeaker)

### Data Protection
- HTTPS enforcement
- CSRF protection on forms
- Input validation and sanitization
- SQL injection prevention through EF parameterized queries

### File Upload Security (Future Extension)
- File type validation
- Size limitations
- Secure file storage (Azure Blob Storage recommended)
- Virus scanning integration

## Performance Considerations

### Server-Side Optimizations
- Entity Framework query optimization
- Response caching where appropriate
- Compression middleware
- CDN for static assets (Future Extension)

### Client-Side Optimizations
- Minimal JavaScript bundle size
- HTMX for partial page updates
- Lazy loading of images
- Progressive web app features

### Database Optimizations
- Proper indexing strategy
- Connection pooling
- Query optimization
- Database connection retry policies

## Scalability Design

### Horizontal Scaling
- Stateless application design
- Load balancer friendly

### Vertical Scaling
- Async/await patterns throughout
- Efficient memory usage
- Database connection pooling

### Caching Strategy
- In-memory caching for frequently accessed data
- Distributed caching for user sessions
- CDN for static content
- Database query result caching

## Monitoring and Observability

### .NET Aspire Integration
- Built-in telemetry collection
- Distributed tracing
- Metrics dashboard
- Health checks

### Application Insights
- Performance monitoring
- Error tracking
- Custom telemetry
- User behavior analytics

### Logging Strategy
- Structured logging with Serilog
- Different log levels for environments
- Correlation IDs for request tracking
- Log aggregation in Azure

## API Design (Future Extension)

### RESTful Principles
- Resource-based URLs
- HTTP verbs for actions
- Consistent response formats
- Proper status codes

### GraphQL Considerations
- Single endpoint for complex queries
- Reduced over-fetching
- Strong typing
- Real-time subscriptions

## Testing Strategy

### Unit Testing
- xUnit framework
- Moq for mocking
- Test coverage reporting
- Fast feedback loop

### Integration Testing
- In-memory database for data layer tests
- End-to-end scenarios

### UI Testing
- Playwright for browser automation (Future Extension)
- Visual regression testing
- Accessibility testing

## Deployment Architecture

### Development Environment
- .NET Aspire for orchestration
  - Local SQL Server or LocalDB
  - Azure Storage Emulator
  - Azurite for Azure Blob Storage
- Hot reload for rapid development

### Staging Environment (Not Implemented Yet)
- Azure App Service (Standard tier)
- Azure SQL Database (Standard tier)
- Application Insights monitoring

### Production Environment
- Azure App Service
- Azure SQL Database
- Azure Communication Services for email notifications
- Azure Monitor (Application Insights)
- Azure Key Vault for application secrets
- Azure Functions for scheduled tasks
- Azure CDN for static content

This architecture provides a solid foundation for the MoreSpeakers.com platform while maintaining flexibility for future enhancements and scalability requirements.