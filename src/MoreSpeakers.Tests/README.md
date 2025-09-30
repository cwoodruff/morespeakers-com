# MoreSpeakers.Tests

Comprehensive test suite for the MoreSpeakers.com application using xUnit.

## Test Categories

### Unit Tests
- **Service Layer Tests**: Tests for `SpeakerService`, `ExpertiseService`, `MentorshipService`
- **Model Tests**: Validation and business logic tests for domain models
- **Page Model Tests**: Tests for Razor Page models

### Integration Tests
- **Database Integration**: Tests Entity Framework configuration and relationships
- **End-to-End Scenarios**: Complex workflows and data operations

## Test Structure

```
MoreSpeakers.Tests/
├── Services/           # Service layer unit tests
├── Models/            # Model validation tests
├── Pages/             # Razor Pages tests
├── Integration/       # Integration tests
├── Utilities/         # Test helpers and data builders
├── TestBase.cs        # Base test class with in-memory database
└── README.md         # This file
```

## Key Testing Libraries

- **xUnit**: Primary testing framework
- **FluentAssertions**: Fluent assertion library for more readable tests
- **Moq**: Mocking framework for dependencies
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Bogus**: Fake data generation for test scenarios
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core testing utilities

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "SpeakerServiceTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in specific category
dotnet test --filter "Category=Unit"
```

## Test Data

The test suite uses:
- **In-memory database** for isolated testing
- **Seeded test data** in `TestBase.cs`
- **Bogus data builders** in `TestDataBuilder.cs` for realistic fake data
- **Custom test fixtures** for specific scenarios

## Test Coverage

The test suite covers:
- ✅ Service layer business logic
- ✅ Data model validation
- ✅ Entity Framework relationships
- ✅ Razor Page models
- ✅ Database constraints and cascading
- ✅ Authentication and authorization scenarios
- ✅ Error handling and edge cases

## Known Issues

Some tests may fail initially due to:
- Entity ID conflicts in seeded data (use unique GUIDs/IDs)
- Validation attribute differences between test and runtime environments
- Missing authentication context in page tests

## Future Enhancements

- Performance testing for database operations
- Integration tests with real database
- UI testing with Selenium or Playwright
- API endpoint testing for future API development
- Load testing for high-traffic scenarios