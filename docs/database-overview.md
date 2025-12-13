# MoreSpeakers.com - Database Overview

## Database Schema Design

The MoreSpeakers.com database is designed using a relational model optimized for the mentorship platform's requirements. The schema supports both new and experienced speakers with flexible profile management and mentorship tracking.

### Entity Relationship Diagram

```
┌──────────────────────────┐            ┌──────────────────────────┐
│   dbo.ExpertiseCategories│<───────────│        dbo.Expertises    │
├──────────────────────────┤     FK     ├──────────────────────────┤
│ PK Id (int)              │            │ PK Id (int)               │
│ Name, Description        │            │ Name, Description         │
│ CreatedDate, IsActive    │            │ CreatedDate, IsActive     │
└──────────────────────────┘            │ ExpertiseCategoryId (FK)  │
                                       └──────────────────────────┘

                                  ┌──────────────────────────┐
                                  │  dbo.MentorshipExpertises│
                                  ├──────────────────────────┤
                                  │ PK/FK MentorshipId (uuid)│───────┐
                                  │ PK/FK ExpertiseId (int)  │───┐   │
                                  └──────────────────────────┘   │   │
                                                                 │   │
                     ┌──────────────────────────┐                │   │
                     │      dbo.Mentorships     │◀───────────────┘   │
                     ├──────────────────────────┤                    │
                     │ PK Id (uuid)             │                    │
                     │ Type, Status, Dates, ... │                    │
                     │ Notes                    │                    │
                     └──────────────────────────┘                    │
                                                                     │
                                                                     ▼
                                                        ┌──────────────────────────┐
                                                        │      dbo.Expertises      │
                                                        └──────────────────────────┘

┌──────────────────────────┐            ┌──────────────────────────┐
│   dbo.SocialMediaSites   │<───────────│  dbo.UserSocialMediaSites│
├──────────────────────────┤     FK     ├──────────────────────────┤
│ PK Id (int)              │            │ PK Id (int)              │
│ Name, Icon, UrlFormat    │            │ SocialMediaSiteId (FK)   │
└──────────────────────────┘            │ UserId (uuid)            │
                                        │ SocialId (varchar)       │
                                        └──────────────────────────┘

┌──────────────────────────┐            ┌──────────────────────────┐
│     dbo.SpeakerTypes     │            │      dbo.SocialMedias    │
├──────────────────────────┤            ├──────────────────────────┤
│ PK Id (int)              │            │ PK Id (int)              │
│ Name, Description        │            │ UserId (uuid)            │
│ CreatedDate              │            │ Platform, Url, IsActive  │
└──────────────────────────┘            │ CreatedDate              │
                                        └──────────────────────────┘

┌──────────────────────────┐
│     dbo.UserExpertises   │───────────────▶ dbo.Expertises.Id
├──────────────────────────┤
│ PK/FK UserId (uuid)      │  (FK to AspNetUsers omitted)
│ PK/FK ExpertiseId (int)  │
│ CreatedDate              │
└──────────────────────────┘

Note:
- All relationships to tables whose names begin with "AspNet" are intentionally omitted from this diagram.
```

## Core Tables

### AspNetUsers Table
The central table storing all user information for both new and experienced speakers.

```sql
create table AspNetUsers
(
    Id                      uniqueidentifier default newid()      not null
        primary key,
    UserName                nvarchar(256),
    NormalizedUserName      nvarchar(256),
    Email                   nvarchar(256),
    NormalizedEmail         nvarchar(256),
    EmailConfirmed          bit              default 0            not null,
    PasswordHash            nvarchar(max),
    SecurityStamp           nvarchar(max),
    ConcurrencyStamp        nvarchar(max),
    PhoneNumber             nvarchar(max)
        constraint CHK_Users_Phone_Format
            check ([PhoneNumber] IS NULL OR
                   len([PhoneNumber]) >= 10 AND len([PhoneNumber]) <= 20 AND [PhoneNumber] like '%[0-9]%'),
    PhoneNumberConfirmed    bit              default 0            not null,
    TwoFactorEnabled        bit              default 0            not null,
    LockoutEnd              datetimeoffset,
    LockoutEnabled          bit              default 0            not null,
    AccessFailedCount       int              default 0            not null,
    FirstName               nvarchar(100)                         not null
        constraint CHK_Users_FirstName_Length
            check (len([FirstName]) >= 2 AND len([FirstName]) <= 100),
    LastName                nvarchar(100)                         not null
        constraint CHK_Users_LastName_Length
            check (len([LastName]) >= 2 AND len([LastName]) <= 100),
    Bio                     nvarchar(max)                         not null
        constraint CHK_Users_Bio_Length
            check (len([Bio]) <= 6000),
    SessionizeUrl           nvarchar(500)
        constraint CHK_Users_SessionizeUrl_Format
            check ([SessionizeUrl] IS NULL OR [SessionizeUrl] like 'http%://%' OR [SessionizeUrl] like 'https%://%'),
    HeadshotUrl             nvarchar(500),
    SpeakerTypeId           int                                   not null
        constraint FK_AspNetUsers_SpeakerTypes
            references SpeakerTypes,
    Goals                   nvarchar(max)                         not null
        constraint CHK_Users_Goals_Length
            check (len([Goals]) <= 2000),
    CreatedDate             datetime2        default getutcdate() not null,
    UpdatedDate             datetime2        default getutcdate() not null,
    IsActive                bit              default 1            not null,
    IsAvailableForMentoring bit              default 1            not null,
    MaxMentees              int              default 2            not null,
    MentorshipFocus         nvarchar(1000)
)
```

### SpeakerTypes Table
Defines whether a user is a new speaker or experienced speaker.

```sql
create table SpeakerTypes
(
    Id          int identity
        primary key,
    Name        nvarchar(50)                   not null
        unique,
    Description nvarchar(200)                  not null,
    CreatedDate datetime2 default getutcdate() not null
)
INSERT INTO SpeakerTypes (Name, Description) VALUES 
('NewSpeaker', 'Aspiring speakers seeking mentorship and guidance'),
('ExperiencedSpeaker', 'Experienced speakers offering mentorship');
```

### Expertise Table
Manages technology and software development expertise areas as reusable tags.

```sql
create table ExpertiseCategories (
                                     Id int primary key not null,
                                     Name nvarchar(100) not null,
                                     Description nvarchar(500),
                                     CreatedDate datetime2 default (sysutcdatetime()) not null,
                                     IsActive bit default ((1)) not null
);
create unique index UQ__Expertis__737584F65139BC94 on ExpertiseCategories (Name);
GO

create table dbo.Expertises (
                                Id int primary key not null,
                                Name nvarchar(100) not null,
                                Description nvarchar(500),
                                CreatedDate datetime2 default (getutcdate()) not null,
                                IsActive bit default ((1)) not null,
                                ExpertiseCategoryId int not null,
                                foreign key (ExpertiseCategoryId) references ExpertiseCategories (Id)
);
create index IX_Expertises_ExpertiseCategoryId on Expertises (ExpertiseCategoryId);
create unique index UQ__Expertis__737584F6EB425532 on Expertises (Name);
GO
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
CREATE TABLE SocialMediaSites
(
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name varchar(50),
    Icon varchar(50),
    UrlFormat varchar(1024)
)

CREATE INDEX IX_SocialMediaSites_Name
    on SocialMediaSites (Name)
go

CREATE TABLE UserSocialMediaSites
(
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER
        constraint FK_UserSocialMediaSites_AspNetUsers_Id
            references AspNetUsers
            on delete cascade,
    SocialMediaSiteId int,
    constraint FK_UserSocialMediaSites_SocialMediaSites_Id foreign key (SocialMediaSiteId) references SocialMediaSites(Id),
    SocialId varchar(1024),
)

CREATE INDEX IX_UserSocialMediaSites_UserId
    on UserSocialMediaSites (UserId)
go
```

### Mentorships Table
Tracks mentorship relationships between new and experienced speakers.

```sql
create table Mentorships
(
    Id                 uniqueidentifier default newid()            not null
        constraint PK_Mentorships
            primary key,
    MentorId           uniqueidentifier                            not null
        constraint FK_Mentorships_AspNetUsers_MentorId
            references AspNetUsers,
    MenteeId           uniqueidentifier                            not null
        constraint FK_Mentorships_AspNetUsers_MenteeId
            references AspNetUsers,
    Type               nvarchar(50)     default 'NewToExperienced' not null
        constraint CHK_Mentorships_Type
            check ([Type] = 'ExperiencedToExperienced' OR [Type] = 'NewToExperienced'),
    Status             nvarchar(20)     default 'Pending'          not null
        constraint CHK_Mentorships_Status
            check ([Status] = 'Cancelled' OR [Status] = 'Declined' OR [Status] = 'Completed' OR [Status] = 'Active' OR
                   [Status] = 'Pending'),
    RequestMessage     nvarchar(1000)   default ''                 not null,
    ResponseMessage    nvarchar(1000),
    PreferredFrequency nvarchar(50),
    RequestedAt        datetime2        default getutcdate()       not null,
    ResponsedAt        datetime2,
    StartedAt          datetime2,
    CompletedAt        datetime2,
    CreatedAt          datetime2        default getutcdate()       not null,
    UpdatedAt          datetime2        default getutcdate()       not null,
    Notes              nvarchar(2000),
    NewSpeakerId       as [MenteeId] persisted not null,
    RequestDate        as [RequestedAt] persisted not null,
    AcceptedDate       as [StartedAt],
    constraint CHK_Mentorships_DifferentUsers
        check ([MenteeId] <> [MentorId])
)
go

create table MentorshipExpertises
(
    MentorshipId uniqueidentifier not null
        constraint FK_MentorshipExpertises_Mentorships_MentorshipId
            references Mentorships
            on delete cascade,
    ExpertiseId  int              not null
        constraint FK_MentorshipExpertises_Expertises_ExpertiseId
            references Expertises
            on delete cascade,
    constraint PK_MentorshipExpertises
        primary key (MentorshipId, ExpertiseId)
)
go

create index IX_MentorshipExpertise_ExpertiseId on MentorshipExpertises (ExpertiseId)
go

create index IX_Mentorships_MentorId on Mentorships (MentorId)
go

create index IX_Mentorships_MenteeId on Mentorships (MenteeId)
go

create index IX_Mentorships_Status on Mentorships (Status)
go

create index IX_Mentorships_Type on Mentorships (Type)
go

create index IX_Mentorships_Status_RequestedAt on Mentorships (Status, RequestedAt)
go

create index IX_Mentorships_MentorId_Status on Mentorships (MentorId, Status)
go

create index IX_Mentorships_MenteeId_Status on Mentorships (MenteeId, Status)
go
```

## Database Constraints and Business Rules

### Data Integrity Rules
1. **Email Uniqueness**: Each user must have a unique email address
2. **Required Fields**: First name, last name, email, phone, bio, and goals are mandatory
3. **Social Media Minimum**: Each user must have at least one social media link
4. **Expertise Requirement**: Each user must have at least one expertise area
5. **Mentorship Logic**: New speakers can only be mentored by experienced speakers

## Local Development Database Setup

The solution uses .NET Aspire to create a local database for development.
This is already configured in the solution and can be used to run the application locally.

## Database Migration Strategy

### Development Workflow
1. Make model changes
2. Create a SQL script with any changes required
3. Review generated migration
4. Update local database
5. Test changes locally
6. Commit sql files

### Production Deployment
1. Deploy application with Sql scripts
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
