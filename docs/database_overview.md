# MoreSpeakers.com - Database Overview

## Database Schema Design

The MoreSpeakers.com database is designed using a relational model optimized for the mentorship platform's requirements. The schema supports both new and experienced speakers with flexible profile management and mentorship tracking.

### Entity Relationship Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     Users       │    │   SpeakerTypes  │    │   Expertise     │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ Id (PK)         │    │ Id (PK)         │    │ Id (PK)         │
│ Email           │    │ Name            │    │ Name            │
│ PasswordHash    │    │ Description     │    │ Description     │
│ FirstName       │    └─────────────────┘    │ CreatedDate     │
│ LastName        │           │               └─────────────────┘
│ PhoneNumber     │           │                        │
│ Bio             │           │                        │
│ SessionizeUrl   │           │                        │
│ HeadshotUrl     │           │                        │
│ SpeakerTypeId(FK)│          │                        │
│ Goals           │           │                        │
│ CreatedDate     │           │                        │
│ UpdatedDate     │           │                        │
└─────────────────┘           │                        │
         │                    │                        │
         │                    │                        │
┌─────────────────┐           │                        │
│  SocialMedia    │           │                        │
├─────────────────┤           │               ┌─────────────────┐
│ Id (PK)         │           │               │ UserExpertise   │
│ UserId (FK)     │───────────┘               ├─────────────────┤
│ Platform        │                           │ UserId (FK)     │
│ Url             │                           │ ExpertiseId (FK)│
│ CreatedDate     │                           └─────────────────┘
└─────────────────┘                                    │
                                                       │
┌─────────────────┐                                    │
│   Mentorships   │                                    │
├─────────────────┤                                    │
│ Id (PK)         │                                    │
│ NewSpeakerId(FK)│────────────────────────────────────┘
│ MentorId (FK)   │
│ Status          │
│ RequestDate     │
│ AcceptedDate    │
│ CompletedDate   │
│ Notes           │
└─────────────────┘
```

## Core Tables

### Users Table
The central table storing all user information for both new and experienced speakers.

```sql
CREATE TABLE Users (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    Email nvarchar(256) NOT NULL UNIQUE,
    PasswordHash nvarchar(max) NOT NULL,
    FirstName nvarchar(100) NOT NULL,
    LastName nvarchar(100) NOT NULL,
    PhoneNumber nvarchar(20) NOT NULL,
    Bio nvarchar(max) NOT NULL,
    SessionizeUrl nvarchar(500) NULL,
    HeadshotUrl nvarchar(500) NULL,
    SpeakerTypeId int NOT NULL,
    Goals nvarchar(max) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Users_SpeakerTypes FOREIGN KEY (SpeakerTypeId) 
        REFERENCES SpeakerTypes(Id)
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_SpeakerType ON Users(SpeakerTypeId);
```

### SpeakerTypes Table
Defines whether a user is a new speaker or experienced speaker.

```sql
CREATE TABLE SpeakerTypes (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(50) NOT NULL UNIQUE,
    Description nvarchar(200) NOT NULL
);

INSERT INTO SpeakerTypes (Name, Description) VALUES 
('NewSpeaker', 'Aspiring speakers seeking mentorship and guidance'),
('ExperiencedSpeaker', 'Experienced speakers offering mentorship');
```

### Expertise Table
Manages technology and software development expertise areas as reusable tags.

```sql
CREATE TABLE Expertise (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL UNIQUE,
    Description nvarchar(500) NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Expertise_Name ON Expertise(Name);
```

### UserExpertise Table
Many-to-many relationship between users and their expertise areas.

```sql
CREATE TABLE UserExpertise (
    UserId uniqueidentifier NOT NULL,
    ExpertiseId int NOT NULL,
    PRIMARY KEY (UserId, ExpertiseId),
    CONSTRAINT FK_UserExpertise_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserExpertise_Expertise FOREIGN KEY (ExpertiseId) 
        REFERENCES Expertise(Id) ON DELETE CASCADE
);
```

### SocialMedia Table
Stores dynamic social media links for each user.

```sql
CREATE TABLE SocialMedia (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId uniqueidentifier NOT NULL,
    Platform nvarchar(50) NOT NULL,
    Url nvarchar(500) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SocialMedia_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_SocialMedia_UserId ON SocialMedia(UserId);
```

### Mentorships Table
Tracks mentorship relationships between new and experienced speakers.

```sql
CREATE TABLE Mentorships (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    NewSpeakerId uniqueidentifier NOT NULL,
    MentorId uniqueidentifier NOT NULL,
    Status nvarchar(20) NOT NULL DEFAULT 'Pending',
    RequestDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
    AcceptedDate datetime2 NULL,
    CompletedDate datetime2 NULL,
    Notes nvarchar(max) NULL,
    CONSTRAINT FK_Mentorships_NewSpeaker FOREIGN KEY (NewSpeakerId) 
        REFERENCES Users(Id),
    CONSTRAINT FK_Mentorships_Mentor FOREIGN KEY (MentorId) 
        REFERENCES Users(Id),
    CONSTRAINT CHK_Mentorships_Status CHECK (Status IN ('Pending', 'Active', 'Completed', 'Cancelled'))
);

CREATE INDEX IX_Mentorships_NewSpeaker ON Mentorships(NewSpeakerId);
CREATE INDEX IX_Mentorships_Mentor ON Mentorships(MentorId);
CREATE INDEX IX_Mentorships_Status ON Mentorships(Status);
```

## Database Constraints and Business Rules

### Data Integrity Rules
1. **Email Uniqueness**: Each user must have a unique email address
2. **Required Fields**: First name, last name, email, phone, bio, and goals are mandatory
3. **Social Media Minimum**: Each user must have at least one social media link
4. **Expertise Requirement**: Each user must have at least one expertise area
5. **Mentorship Logic**: New speakers can only be mentored by experienced speakers

### Validation Constraints
```sql
-- Bio length constraint (max 1000 words ≈ 6000 characters)
ALTER TABLE Users ADD CONSTRAINT CHK_Users_Bio_Length 
    CHECK (LEN(Bio) <= 6000);

-- Phone number format (basic validation)
ALTER TABLE Users ADD CONSTRAINT CHK_Users_Phone_Format 
    CHECK (PhoneNumber LIKE '[0-9+()-. ]*');

-- URL validation for social media
ALTER TABLE SocialMedia ADD CONSTRAINT CHK_SocialMedia_Url_Format 
    CHECK (Url LIKE 'http%://%' OR Url LIKE 'https%://%');
```

## Entity Framework Models

### User Entity
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? SessionizeUrl { get; set; }
    public string? HeadshotUrl { get; set; }
    public int SpeakerTypeId { get; set; }
    public string Goals { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    // Navigation properties
    public SpeakerType SpeakerType { get; set; } = null!;
    public ICollection<SocialMedia> SocialMediaLinks { get; set; } = new List<SocialMedia>();
    public ICollection<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public ICollection<Mentorship> MentorshipsAsMentor { get; set; } = new List<Mentorship>();
    public ICollection<Mentorship> MentorshipsAsNewSpeaker { get; set; } = new List<Mentorship>();
}
```

## Local Development Database Setup

The solution uses .NET Aspire to create a local database for development.
This is already configured in the solution and can be used to run the application locally.

## Database Migration Strategy

### Development Workflow
1. Make model changes
2. Add migration: `dotnet ef migrations add MigrationName`
3. Review generated migration
4. Update local database: `dotnet ef database update`
5. Test changes locally
6. Commit migration files

### Production Deployment
1. Deploy application with new migrations
2. Application startup automatically applies pending migrations
3. Monitor for migration errors
4. Rollback plan if needed

### Migration Best Practices
- Always review generated migrations
- Test migrations on staging environment first
- Keep migrations small and focused
- Add appropriate indexes for performance
- Consider downtime for large schema changes

## Performance Optimization

### Indexing Strategy
```sql
-- Frequently queried columns
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_SpeakerType ON Users(SpeakerTypeId);
CREATE INDEX IX_Mentorships_Status_Date ON Mentorships(Status, RequestDate);

-- Composite indexes for common queries
CREATE INDEX IX_UserExpertise_Composite ON UserExpertise(ExpertiseId, UserId);
CREATE INDEX IX_Mentorships_Mentor_Status ON Mentorships(MentorId, Status);
```

### Query Optimization
- Use Entity Framework's `Include()` for eager loading
- Implement pagination for large result sets
- Use compiled queries for frequently executed queries
- Monitor query execution plans

### Connection Management
- Configure connection pooling in application
- Use connection resiliency for cloud environments
- Monitor connection pool metrics
