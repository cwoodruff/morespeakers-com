-- MoreSpeakers.com Database Schema
-- This script creates the complete database schema for the MoreSpeakers platform
-- Compatible with SQL Server 2019+ and Azure SQL Database

-- =============================================
-- Database Creation (uncomment for new database)
-- =============================================
/*
USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MoreSpeakers')
BEGIN
    CREATE DATABASE MoreSpeakers
    COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO
*/

USE MoreSpeakers;
GO

-- =============================================
-- Drop existing tables (for clean recreation)
-- =============================================
IF OBJECT_ID('UserExpertise', 'U') IS NOT NULL DROP TABLE UserExpertise;
IF OBJECT_ID('SocialMedia', 'U') IS NOT NULL DROP TABLE SocialMedia;
IF OBJECT_ID('Mentorships', 'U') IS NOT NULL DROP TABLE Mentorships;
IF OBJECT_ID('AspNetUserRoles', 'U') IS NOT NULL DROP TABLE AspNetUserRoles;
IF OBJECT_ID('AspNetUserClaims', 'U') IS NOT NULL DROP TABLE AspNetUserClaims;
IF OBJECT_ID('AspNetUserLogins', 'U') IS NOT NULL DROP TABLE AspNetUserLogins;
IF OBJECT_ID('AspNetUserTokens', 'U') IS NOT NULL DROP TABLE AspNetUserTokens;
IF OBJECT_ID('AspNetRoleClaims', 'U') IS NOT NULL DROP TABLE AspNetRoleClaims;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('AspNetUsers', 'U') IS NOT NULL DROP TABLE AspNetUsers;
IF OBJECT_ID('AspNetRoles', 'U') IS NOT NULL DROP TABLE AspNetRoles;
IF OBJECT_ID('SpeakerTypes', 'U') IS NOT NULL DROP TABLE SpeakerTypes;
IF OBJECT_ID('Expertise', 'U') IS NOT NULL DROP TABLE Expertise;
GO

-- =============================================
-- Create SpeakerTypes Table
-- =============================================
CREATE TABLE SpeakerTypes (
                              Id int IDENTITY(1,1) PRIMARY KEY,
                              Name nvarchar(50) NOT NULL UNIQUE,
                              Description nvarchar(200) NOT NULL,
                              CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Index for performance
CREATE INDEX IX_SpeakerTypes_Name ON SpeakerTypes(Name);
GO

-- =============================================
-- Create Expertise Table
-- =============================================
CREATE TABLE Expertise (
                           Id int IDENTITY(1,1) PRIMARY KEY,
                           Name nvarchar(100) NOT NULL UNIQUE,
                           Description nvarchar(500) NULL,
                           CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
                           IsActive bit NOT NULL DEFAULT 1
);

-- Index for performance
CREATE INDEX IX_Expertise_Name ON Expertise(Name);
CREATE INDEX IX_Expertise_Active ON Expertise(IsActive) WHERE IsActive = 1;
GO

-- =============================================
-- Create ASP.NET Identity Tables
-- =============================================

-- AspNetRoles
CREATE TABLE AspNetRoles (
                             Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
                             Name nvarchar(256) NULL,
                             NormalizedName nvarchar(256) NULL,
                             ConcurrencyStamp nvarchar(max) NULL
);

CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
GO

-- AspNetUsers (Extended for MoreSpeakers)
CREATE TABLE AspNetUsers (
                             Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
                             UserName nvarchar(256) NULL,
                             NormalizedUserName nvarchar(256) NULL,
                             Email nvarchar(256) NULL,
                             NormalizedEmail nvarchar(256) NULL,
                             EmailConfirmed bit NOT NULL DEFAULT 0,
                             PasswordHash nvarchar(max) NULL,
                             SecurityStamp nvarchar(max) NULL,
                             ConcurrencyStamp nvarchar(max) NULL,
                             PhoneNumber nvarchar(max) NULL,
                             PhoneNumberConfirmed bit NOT NULL DEFAULT 0,
                             TwoFactorEnabled bit NOT NULL DEFAULT 0,
                             LockoutEnd datetimeoffset(7) NULL,
                             LockoutEnabled bit NOT NULL DEFAULT 0,
                             AccessFailedCount int NOT NULL DEFAULT 0,

    -- Extended properties for MoreSpeakers
                             FirstName nvarchar(100) NOT NULL,
                             LastName nvarchar(100) NOT NULL,
                             Bio nvarchar(max) NOT NULL,
                             SessionizeUrl nvarchar(500) NULL,
                             HeadshotUrl nvarchar(500) NULL,
                             SpeakerTypeId int NOT NULL,
                             Goals nvarchar(max) NOT NULL,
                             CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
                             UpdatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
                             IsActive bit NOT NULL DEFAULT 1,

                             CONSTRAINT FK_AspNetUsers_SpeakerTypes FOREIGN KEY (SpeakerTypeId)
                                 REFERENCES SpeakerTypes(Id)
);

-- Indexes for ASP.NET Identity
CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
CREATE INDEX EmailIndex ON AspNetUsers(NormalizedEmail);

-- Additional indexes for MoreSpeakers functionality
CREATE INDEX IX_AspNetUsers_SpeakerType ON AspNetUsers(SpeakerTypeId);
CREATE INDEX IX_AspNetUsers_CreatedDate ON AspNetUsers(CreatedDate);
CREATE INDEX IX_AspNetUsers_Active ON AspNetUsers(IsActive) WHERE IsActive = 1;
CREATE INDEX IX_AspNetUsers_FullName ON AspNetUsers(FirstName, LastName);
GO

-- AspNetUserRoles
CREATE TABLE AspNetUserRoles (
                                 UserId uniqueidentifier NOT NULL,
                                 RoleId uniqueidentifier NOT NULL,
                                 PRIMARY KEY (UserId, RoleId),
                                 CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId)
                                     REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
                                 CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId)
                                     REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles(RoleId);
GO

-- AspNetUserClaims
CREATE TABLE AspNetUserClaims (
                                  Id int IDENTITY(1,1) PRIMARY KEY,
                                  UserId uniqueidentifier NOT NULL,
                                  ClaimType nvarchar(max) NULL,
                                  ClaimValue nvarchar(max) NULL,
                                  CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId)
                                      REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims(UserId);
GO

-- AspNetUserLogins
CREATE TABLE AspNetUserLogins (
                                  LoginProvider nvarchar(450) NOT NULL,
                                  ProviderKey nvarchar(450) NOT NULL,
                                  ProviderDisplayName nvarchar(max) NULL,
                                  UserId uniqueidentifier NOT NULL,
                                  PRIMARY KEY (LoginProvider, ProviderKey),
                                  CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId)
                                      REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins(UserId);
GO

-- AspNetUserTokens
CREATE TABLE AspNetUserTokens (
                                  UserId uniqueidentifier NOT NULL,
                                  LoginProvider nvarchar(450) NOT NULL,
                                  Name nvarchar(450) NOT NULL,
                                  Value nvarchar(max) NULL,
                                  PRIMARY KEY (UserId, LoginProvider, Name),
                                  CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId)
                                      REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- AspNetRoleClaims
CREATE TABLE AspNetRoleClaims (
                                  Id int IDENTITY(1,1) PRIMARY KEY,
                                  RoleId uniqueidentifier NOT NULL,
                                  ClaimType nvarchar(max) NULL,
                                  ClaimValue nvarchar(max) NULL,
                                  CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId)
                                      REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims(RoleId);
GO

-- =============================================
-- Create UserExpertise Junction Table
-- =============================================
CREATE TABLE UserExpertise (
                               UserId uniqueidentifier NOT NULL,
                               ExpertiseId int NOT NULL,
                               CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
                               PRIMARY KEY (UserId, ExpertiseId),
                               CONSTRAINT FK_UserExpertise_Users FOREIGN KEY (UserId)
                                   REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                               CONSTRAINT FK_UserExpertise_Expertise FOREIGN KEY (ExpertiseId)
                                   REFERENCES Expertise(Id) ON DELETE CASCADE
);

-- Indexes for efficient querying
CREATE INDEX IX_UserExpertise_ExpertiseId ON UserExpertise(ExpertiseId);
CREATE INDEX IX_UserExpertise_CreatedDate ON UserExpertise(CreatedDate);
GO

-- =============================================
-- Create SocialMedia Table
-- =============================================
CREATE TABLE SocialMedia (
                             Id int IDENTITY(1,1) PRIMARY KEY,
                             UserId uniqueidentifier NOT NULL,
                             Platform nvarchar(50) NOT NULL,
                             Url nvarchar(500) NOT NULL,
                             CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
                             IsActive bit NOT NULL DEFAULT 1,

                             CONSTRAINT FK_SocialMedia_Users FOREIGN KEY (UserId)
                                 REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
                             CONSTRAINT CHK_SocialMedia_Url_Format CHECK (Url LIKE 'http%://%' OR Url LIKE 'https%://%')
);

-- Indexes for performance
CREATE INDEX IX_SocialMedia_UserId ON SocialMedia(UserId);
CREATE INDEX IX_SocialMedia_Platform ON SocialMedia(Platform);
CREATE INDEX IX_SocialMedia_Active ON SocialMedia(IsActive) WHERE IsActive = 1;
GO

-- =============================================
-- Create Mentorships Table
-- =============================================
CREATE TABLE Mentorships (
                             Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
                             NewSpeakerId uniqueidentifier NOT NULL,
                             MentorId uniqueidentifier NOT NULL,
                             Status nvarchar(20) NOT NULL DEFAULT 'Pending',
                             RequestDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
                             AcceptedDate datetime2 NULL,
                             CompletedDate datetime2 NULL,
                             CancelledDate datetime2 NULL,
                             Notes nvarchar(max) NULL,
                             CancellationReason nvarchar(1000) NULL,
                             CreatedBy uniqueidentifier NULL,
                             UpdatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),

                             CONSTRAINT FK_Mentorships_NewSpeaker FOREIGN KEY (NewSpeakerId)
                                 REFERENCES AspNetUsers(Id),
                             CONSTRAINT FK_Mentorships_Mentor FOREIGN KEY (MentorId)
                                 REFERENCES AspNetUsers(Id),
                             CONSTRAINT CHK_Mentorships_Status CHECK (Status IN ('Pending', 'Active', 'Completed', 'Cancelled')),
                             CONSTRAINT CHK_Mentorships_DifferentUsers CHECK (NewSpeakerId != MentorId),
                             CONSTRAINT CHK_Mentorships_AcceptedDate CHECK (AcceptedDate IS NULL OR AcceptedDate >= RequestDate),
                             CONSTRAINT CHK_Mentorships_CompletedDate CHECK (CompletedDate IS NULL OR CompletedDate >= COALESCE(AcceptedDate, RequestDate)),
                             CONSTRAINT CHK_Mentorships_CancelledDate CHECK (CancelledDate IS NULL OR CancelledDate >= RequestDate)
);

-- Indexes for efficient querying
CREATE INDEX IX_Mentorships_NewSpeaker ON Mentorships(NewSpeakerId);
CREATE INDEX IX_Mentorships_Mentor ON Mentorships(MentorId);
CREATE INDEX IX_Mentorships_Status ON Mentorships(Status);
CREATE INDEX IX_Mentorships_RequestDate ON Mentorships(RequestDate);
CREATE INDEX IX_Mentorships_Status_Date ON Mentorships(Status, RequestDate);
CREATE INDEX IX_Mentorships_Active ON Mentorships(Status, AcceptedDate) WHERE Status = 'Active';
GO

-- =============================================
-- Create Additional Constraints
-- =============================================

-- Add check constraints for data integrity
ALTER TABLE AspNetUsers ADD CONSTRAINT CHK_Users_Bio_Length
    CHECK (LEN(Bio) <= 6000);

ALTER TABLE AspNetUsers ADD CONSTRAINT CHK_Users_Phone_Format
    CHECK (PhoneNumber IS NULL OR
           (LEN(PhoneNumber) >= 10 AND LEN(PhoneNumber) <= 20 AND
            PhoneNumber LIKE '%[0-9]%'));

ALTER TABLE AspNetUsers ADD CONSTRAINT CHK_Users_FirstName_Length
    CHECK (LEN(FirstName) >= 2 AND LEN(FirstName) <= 100);

ALTER TABLE AspNetUsers ADD CONSTRAINT CHK_Users_LastName_Length
    CHECK (LEN(LastName) >= 2 AND LEN(LastName) <= 100);

ALTER TABLE AspNetUsers ADD CONSTRAINT CHK_Users_Goals_Length
    CHECK (LEN(Goals) <= 2000);

ALTER TABLE AspNetUsers ADD CONSTRAINT CHK_Users_SessionizeUrl_Format
    CHECK (SessionizeUrl IS NULL OR SessionizeUrl LIKE 'http%://%' OR SessionizeUrl LIKE 'https%://%');
GO

-- =============================================
-- Insert Initial Data
-- =============================================

-- Insert SpeakerTypes
INSERT INTO SpeakerTypes (Name, Description) VALUES
                                                 ('NewSpeaker', 'Aspiring speakers seeking mentorship and guidance'),
                                                 ('ExperiencedSpeaker', 'Experienced speakers offering mentorship');
GO

-- Insert common Expertise areas
SET IDENTITY_INSERT Expertise ON;

INSERT INTO Expertise (Id, Name, Description) VALUES
                                                  (1, 'C#', 'C# programming language and .NET ecosystem'),
                                                  (2, '.NET', '.NET framework and .NET Core development'),
                                                  (3, 'ASP.NET Core', 'ASP.NET Core web application development'),
                                                  (4, 'Blazor', 'Blazor web development framework'),
                                                  (5, 'Azure', 'Microsoft Azure cloud platform and services'),
                                                  (6, 'JavaScript', 'JavaScript programming and web development'),
                                                  (7, 'TypeScript', 'TypeScript language and development'),
                                                  (8, 'React', 'React.js frontend development'),
                                                  (9, 'Angular', 'Angular framework development'),
                                                  (10, 'Vue.js', 'Vue.js frontend framework'),
                                                  (11, 'Node.js', 'Node.js backend development'),
                                                  (12, 'Python', 'Python programming language'),
                                                  (13, 'Java', 'Java programming language and ecosystem'),
                                                  (14, 'Kubernetes', 'Kubernetes container orchestration'),
                                                  (15, 'Docker', 'Docker containerization technology'),
                                                  (16, 'DevOps', 'DevOps practices and methodologies'),
                                                  (17, 'CI/CD', 'Continuous Integration and Continuous Deployment'),
                                                  (18, 'Machine Learning', 'Machine Learning and AI development'),
                                                  (19, 'Data Science', 'Data Science and analytics'),
                                                  (20, 'SQL Server', 'Microsoft SQL Server database'),
                                                  (21, 'PostgreSQL', 'PostgreSQL database development'),
                                                  (22, 'MongoDB', 'MongoDB NoSQL database'),
                                                  (23, 'Redis', 'Redis caching and data store'),
                                                  (24, 'Microservices', 'Microservices architecture and development'),
                                                  (25, 'API Design', 'API design and development best practices'),
                                                  (26, 'GraphQL', 'GraphQL API development'),
                                                  (27, 'REST', 'RESTful API design and development'),
                                                  (28, 'Security', 'Application and system security'),
                                                  (29, 'Performance Optimization', 'Application performance tuning'),
                                                  (30, 'Testing', 'Software testing methodologies'),
                                                  (31, 'TDD', 'Test-Driven Development'),
                                                  (32, 'BDD', 'Behavior-Driven Development'),
                                                  (33, 'Agile', 'Agile development methodologies'),
                                                  (34, 'Scrum', 'Scrum framework and practices'),
                                                  (35, 'Leadership', 'Technical leadership and management'),
                                                  (36, 'Architecture', 'Software architecture and design'),
                                                  (37, 'Design Patterns', 'Software design patterns'),
                                                  (38, 'Clean Code', 'Clean code principles and practices'),
                                                  (39, 'SOLID Principles', 'SOLID design principles'),
                                                  (40, 'Domain-Driven Design', 'Domain-Driven Design methodology');

SET IDENTITY_INSERT Expertise OFF;
GO

-- Insert ASP.NET Identity Roles
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
                                                                         (NEWID(), 'NewSpeaker', 'NEWSPEAKER', NEWID()),
                                                                         (NEWID(), 'ExperiencedSpeaker', 'EXPERIENCEDSPEAKER', NEWID()),
                                                                         (NEWID(), 'Administrator', 'ADMINISTRATOR', NEWID());
GO

-- =============================================
-- Create Views for Common Queries
-- =============================================

-- View for speaker details with expertise
CREATE VIEW vw_SpeakerDetails AS
SELECT
    u.Id,
    u.FirstName,
    u.LastName,
    u.FirstName + ' ' + u.LastName AS FullName,
    u.Email,
    u.PhoneNumber,
    u.Bio,
    u.SessionizeUrl,
    u.HeadshotUrl,
    u.Goals,
    u.CreatedDate,
    u.UpdatedDate,
    st.Name AS SpeakerType,
    st.Description AS SpeakerTypeDescription,
    COUNT(ue.ExpertiseId) AS ExpertiseCount,
    COUNT(sm.Id) AS SocialMediaCount,
    COUNT(m1.Id) AS MentorshipsAsMentor,
    COUNT(m2.Id) AS MentorshipsAsNewSpeaker
FROM AspNetUsers u
         INNER JOIN SpeakerTypes st ON u.SpeakerTypeId = st.Id
         LEFT JOIN UserExpertise ue ON u.Id = ue.UserId
         LEFT JOIN SocialMedia sm ON u.Id = sm.UserId AND sm.IsActive = 1
         LEFT JOIN Mentorships m1 ON u.Id = m1.MentorId
         LEFT JOIN Mentorships m2 ON u.Id = m2.NewSpeakerId
WHERE u.IsActive = 1
GROUP BY u.Id, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.Bio,
         u.SessionizeUrl, u.HeadshotUrl, u.Goals, u.CreatedDate, u.UpdatedDate,
         st.Name, st.Description;
GO

-- View for active mentorships with speaker details
CREATE VIEW vw_ActiveMentorships AS
SELECT
    m.Id,
    m.Status,
    m.RequestDate,
    m.AcceptedDate,
    m.Notes,
    ns.FirstName + ' ' + ns.LastName AS NewSpeakerName,
    ns.Email AS NewSpeakerEmail,
    ns.Id AS NewSpeakerId,
    mentor.FirstName + ' ' + mentor.LastName AS MentorName,
    mentor.Email AS MentorEmail,
    mentor.Id AS MentorId,
    DATEDIFF(day, COALESCE(m.AcceptedDate, m.RequestDate), GETUTCDATE()) AS DaysActive
FROM Mentorships m
         INNER JOIN AspNetUsers ns ON m.NewSpeakerId = ns.Id
         INNER JOIN AspNetUsers mentor ON m.MentorId = mentor.Id
WHERE m.Status IN ('Pending', 'Active')
  AND ns.IsActive = 1
  AND mentor.IsActive = 1;
GO

-- View for expertise popularity
CREATE VIEW vw_ExpertisePopularity AS
SELECT
    e.Id,
    e.Name,
    e.Description,
    COUNT(ue.UserId) AS UserCount,
    COUNT(CASE WHEN u.SpeakerTypeId = 1 THEN 1 END) AS NewSpeakerCount,
    COUNT(CASE WHEN u.SpeakerTypeId = 2 THEN 1 END) AS ExperiencedSpeakerCount
FROM Expertise e
         LEFT JOIN UserExpertise ue ON e.Id = ue.ExpertiseId
         LEFT JOIN AspNetUsers u ON ue.UserId = u.Id AND u.IsActive = 1
WHERE e.IsActive = 1
GROUP BY e.Id, e.Name, e.Description;
GO

-- =============================================
-- Create Stored Procedures
-- =============================================

-- Procedure to get speaker recommendations
CREATE PROCEDURE sp_GetSpeakerRecommendations
    @UserId uniqueidentifier,
    @MaxResults int = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Get speakers with similar expertise
    SELECT TOP (@MaxResults)
        s.Id,
        s.FirstName + ' ' + s.LastName AS FullName,
        s.Email,
        s.Bio,
        s.HeadshotUrl,
        s.SpeakerTypeId,
        COUNT(DISTINCT ue2.ExpertiseId) AS CommonExpertiseCount,
        COUNT(DISTINCT ue3.ExpertiseId) AS TotalExpertiseCount
    FROM AspNetUsers s
             INNER JOIN UserExpertise ue2 ON s.Id = ue2.UserId
             INNER JOIN UserExpertise ue1 ON ue2.ExpertiseId = ue1.ExpertiseId
             LEFT JOIN UserExpertise ue3 ON s.Id = ue3.UserId
    WHERE ue1.UserId = @UserId
      AND s.Id != @UserId
      AND s.IsActive = 1
    GROUP BY s.Id, s.FirstName, s.LastName, s.Email, s.Bio, s.HeadshotUrl, s.SpeakerTypeId
    ORDER BY COUNT(DISTINCT ue2.ExpertiseId) DESC, s.CreatedDate DESC;
END;
GO

-- Procedure to get mentorship statistics
CREATE PROCEDURE sp_GetMentorshipStatistics
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        'Total New Speakers' AS Metric,
        COUNT(*) AS Value
    FROM AspNetUsers
    WHERE SpeakerTypeId = 1 AND IsActive = 1

    UNION ALL

    SELECT
        'Total Experienced Speakers' AS Metric,
        COUNT(*) AS Value
    FROM AspNetUsers
    WHERE SpeakerTypeId = 2 AND IsActive = 1

    UNION ALL

    SELECT
        'Active Mentorships' AS Metric,
        COUNT(*) AS Value
    FROM Mentorships
    WHERE Status = 'Active'

    UNION ALL

    SELECT
        'Pending Mentorship Requests' AS Metric,
        COUNT(*) AS Value
    FROM Mentorships
    WHERE Status = 'Pending'

    UNION ALL

    SELECT
        'Completed Mentorships' AS Metric,
        COUNT(*) AS Value
    FROM Mentorships
    WHERE Status = 'Completed';
END;
GO

-- =============================================
-- Create Triggers
-- =============================================

-- Trigger to update UpdatedDate when user is modified
CREATE TRIGGER tr_Users_UpdateTimestamp
    ON AspNetUsers
    AFTER UPDATE
    AS
BEGIN
    SET NOCOUNT ON;

    UPDATE AspNetUsers
    SET UpdatedDate = GETUTCDATE()
    FROM AspNetUsers u
             INNER JOIN inserted i ON u.Id = i.Id;
END;
GO

-- Trigger to update mentorship UpdatedDate
CREATE TRIGGER tr_Mentorships_UpdateTimestamp
    ON Mentorships
    AFTER UPDATE
    AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Mentorships
    SET UpdatedDate = GETUTCDATE()
    FROM Mentorships m
             INNER JOIN inserted i ON m.Id = i.Id;
END;
GO

-- =============================================
-- Create Functions
-- =============================================

-- Function to calculate user's expertise match percentage
CREATE FUNCTION fn_CalculateExpertiseMatch
(
    @UserId1 uniqueidentifier,
    @UserId2 uniqueidentifier
)
    RETURNS decimal(5,2)
AS
BEGIN
    DECLARE @CommonCount int;
    DECLARE @TotalUniqueCount int;
    DECLARE @MatchPercentage decimal(5,2);

    -- Count common expertise areas
    SELECT @CommonCount = COUNT(*)
    FROM UserExpertise ue1
             INNER JOIN UserExpertise ue2 ON ue1.ExpertiseId = ue2.ExpertiseId
    WHERE ue1.UserId = @UserId1 AND ue2.UserId = @UserId2;

    -- Count total unique expertise areas between both users
    SELECT @TotalUniqueCount = COUNT(DISTINCT ExpertiseId)
    FROM UserExpertise
    WHERE UserId IN (@UserId1, @UserId2);

    -- Calculate percentage
    IF @TotalUniqueCount > 0
        SET @MatchPercentage = (@CommonCount * 100.0) / @TotalUniqueCount;
    ELSE
        SET @MatchPercentage = 0;

    RETURN @MatchPercentage;
END;
GO

-- =============================================
-- Sample Data (Optional - for testing)
-- =============================================

/*
-- Uncomment this section to insert sample data for testing
-- Note: This creates test users with simple passwords - use proper hashing in production

-- Get role IDs
DECLARE @NewSpeakerRoleId uniqueidentifier = (SELECT Id FROM AspNetRoles WHERE Name = 'NewSpeaker');
DECLARE @ExperiencedSpeakerRoleId uniqueidentifier = (SELECT Id FROM AspNetRoles WHERE Name = 'ExperiencedSpeaker');

-- Generate user IDs
DECLARE @NewSpeakerId uniqueidentifier = NEWID();
DECLARE @ExperiencedSpeakerId uniqueidentifier = NEWID();

-- Sample New Speaker
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    FirstName, LastName, Bio, Goals, SpeakerTypeId, PhoneNumber,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
)
VALUES (
           @NewSpeakerId,
           'jane.doe@example.com',
           'JANE.DOE@EXAMPLE.COM',
           'jane.doe@example.com',
           'JANE.DOE@EXAMPLE.COM',
           1,
           'AQAAAAEAACcQAAAAEK8VHjlhg0p5R8QQQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5k=', -- Sample hash for 'Password123!'
           NEWID(),
           NEWID(),
           'Jane',
           'Doe',
           'Passionate software developer with 3 years of experience in web development. Looking to share my knowledge and learn from the community. I have been working primarily with .NET technologies and am eager to start speaking at local meetups.',
           'I want to improve my public speaking skills and learn how to create engaging technical presentations. My goal is to speak at my first conference within the next year.',
           1,
           '6165550123',
           0, 0, 1, 0
       );

-- Sample Experienced Speaker
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    FirstName, LastName, Bio, Goals, SpeakerTypeId, PhoneNumber,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
)
VALUES (
           @ExperiencedSpeakerId,
           'john.smith@example.com',
           'JOHN.SMITH@EXAMPLE.COM',
           'john.smith@example.com',
           'JOHN.SMITH@EXAMPLE.COM',
           1,
           'AQAAAAEAACcQAAAAEK8VHjlhg0p5R8QQQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5k=', -- Sample hash for 'Password123!'
           NEWID(),
           NEWID(),
           'John',
           'Smith',
           'Senior software architect with 10+ years of experience in building scalable applications. Regular speaker at conferences and meetups worldwide. I have spoken at over 50 events and mentored dozens of new speakers.',
           'I want to help new speakers find their voice and build confidence in public speaking. I enjoy sharing knowledge about software architecture, clean code, and leadership.',
           2,
           '2485550456',
           0, 0, 1, 0
       );

-- Additional sample users
DECLARE @NewSpeaker2Id uniqueidentifier = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    FirstName, LastName, Bio, Goals, SpeakerTypeId, PhoneNumber,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
)
VALUES (
           @NewSpeaker2Id,
           'alex.johnson@example.com',
           'ALEX.JOHNSON@EXAMPLE.COM',
           'alex.johnson@example.com',
           'ALEX.JOHNSON@EXAMPLE.COM',
           1,
           'AQAAAAEAACcQAAAAEK8VHjlhg0p5R8QQQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5k=',
           NEWID(),
           NEWID(),
           'Alex',
           'Johnson',
           'Full-stack developer with expertise in React and Node.js. Recently transitioned from backend development and excited to share my journey with others.',
           'I want to learn how to present technical concepts in an accessible way and eventually speak at JavaScript conferences.',
           1,
           '616555-0789',
           0, 0, 1, 0
       );

DECLARE @ExperiencedSpeaker2Id uniqueidentifier = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    FirstName, LastName, Bio, Goals, SpeakerTypeId, PhoneNumber,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    SessionizeUrl
)
VALUES (
           @ExperiencedSpeaker2Id,
           'sarah.chen@example.com',
           'SARAH.CHEN@EXAMPLE.COM',
           'sarah.chen@example.com',
           'SARAH.CHEN@EXAMPLE.COM',
           1,
           'AQAAAAEAACcQAAAAEK8VHjlhg0p5R8QQQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5kQ5k2rJ5zJ5k=',
           NEWID(),
           NEWID(),
           'Sarah',
           'Chen',
           'DevOps engineer and cloud architect with extensive experience in Azure and Kubernetes. Passionate about helping teams adopt modern deployment practices and improve their development workflows.',
           'I enjoy mentoring developers who want to learn about DevOps practices and cloud technologies. I can help with conference abstract writing and presentation skills.',
           2,
           '+1-555-0321',
           0, 0, 1, 0,
           'https://sessionize.com/sarah-chen'
       );

-- Assign roles to users
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES
                                                 (@NewSpeakerId, @NewSpeakerRoleId),
                                                 (@NewSpeaker2Id, @NewSpeakerRoleId),
                                                 (@ExperiencedSpeakerId, @ExperiencedSpeakerRoleId),
                                                 (@ExperiencedSpeaker2Id, @ExperiencedSpeakerRoleId);

-- Sample expertise assignments
INSERT INTO UserExpertise (UserId, ExpertiseId) VALUES
-- Jane Doe (New Speaker) - .NET focused
(@NewSpeakerId, 1),  -- C#
(@NewSpeakerId, 2),  -- .NET
(@NewSpeakerId, 3),  -- ASP.NET Core
(@NewSpeakerId, 6),  -- JavaScript
(@NewSpeakerId, 20), -- SQL Server

-- Alex Johnson (New Speaker) - JavaScript focused
(@NewSpeaker2Id, 6),  -- JavaScript
(@NewSpeaker2Id, 8),  -- React
(@NewSpeaker2Id, 11), -- Node.js
(@NewSpeaker2Id, 25), -- API Design
(@NewSpeaker2Id, 30), -- Testing

-- John Smith (Experienced Speaker) - Architecture focused
(@ExperiencedSpeakerId, 1),  -- C#
(@ExperiencedSpeakerId, 2),  -- .NET
(@ExperiencedSpeakerId, 36), -- Architecture
(@ExperiencedSpeakerId, 35), -- Leadership
(@ExperiencedSpeakerId, 37), -- Design Patterns
(@ExperiencedSpeakerId, 38), -- Clean Code
(@ExperiencedSpeakerId, 39), -- SOLID Principles

-- Sarah Chen (Experienced Speaker) - DevOps focused
(@ExperiencedSpeaker2Id, 5),  -- Azure
(@ExperiencedSpeaker2Id, 14), -- Kubernetes
(@ExperiencedSpeaker2Id, 15), -- Docker
(@ExperiencedSpeaker2Id, 16), -- DevOps
(@ExperiencedSpeaker2Id, 17), -- CI/CD
(@ExperiencedSpeaker2Id, 24); -- Microservices

-- Sample social media links
INSERT INTO SocialMedia (UserId, Platform, Url) VALUES
-- Jane Doe
(@NewSpeakerId, 'LinkedIn', 'https://linkedin.com/in/janedoe'),
(@NewSpeakerId, 'GitHub', 'https://github.com/janedoe'),
(@NewSpeakerId, 'Website', 'https://janedoe.dev'),

-- Alex Johnson
(@NewSpeaker2Id, 'LinkedIn', 'https://linkedin.com/in/alexjohnson'),
(@NewSpeaker2Id, 'GitHub', 'https://github.com/alexjohnson'),
(@NewSpeaker2Id, 'Twitter', 'https://twitter.com/alexjohnsondev'),

-- John Smith
(@ExperiencedSpeakerId, 'LinkedIn', 'https://linkedin.com/in/johnsmith'),
(@ExperiencedSpeakerId, 'Twitter', 'https://twitter.com/johnsmitharch'),
(@ExperiencedSpeakerId, 'Website', 'https://johnsmith.tech'),
(@ExperiencedSpeakerId, 'Blog', 'https://blog.johnsmith.tech'),

-- Sarah Chen
(@ExperiencedSpeaker2Id, 'LinkedIn', 'https://linkedin.com/in/sarahchen'),
(@ExperiencedSpeaker2Id, 'GitHub', 'https://github.com/sarahchen'),
(@ExperiencedSpeaker2Id, 'Twitter', 'https://twitter.com/sarahchendevops'),
(@ExperiencedSpeaker2Id, 'YouTube', 'https://youtube.com/c/sarahchentech');

-- Sample mentorships
INSERT INTO Mentorships (Id, NewSpeakerId, MentorId, Status, Notes, RequestDate, AcceptedDate)
VALUES
    (NEWID(), @NewSpeakerId, @ExperiencedSpeakerId, 'Active',
     'Focus on presentation structure and audience engagement. Jane wants to speak about clean architecture patterns.',
     DATEADD(day, -50, GETUTCDATE()), DATEADD(day, -5, GETUTCDATE())),

    (NEWID(), @NewSpeaker2Id, @ExperiencedSpeaker2Id, 'Pending',
     'Alex is interested in learning about speaking at JavaScript conferences and would like help with abstract writing.',
     DATEADD(day, -40, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE())),

    (NEWID(), @NewSpeakerId, @ExperiencedSpeaker2Id, 'Completed',
     'Helped Jane with her first meetup presentation about ASP.NET Core deployment. Very successful!',
     DATEADD(day, -60, GETUTCDATE()), DATEADD(day, -9, GETUTCDATE()))

-- Update the completed mentorship with completion date
UPDATE Mentorships
SET CompletedDate = DATEADD(day, -15, GETUTCDATE()),
    Status = 'Completed'
WHERE NewSpeakerId = @NewSpeakerId
  AND MentorId = @ExperiencedSpeaker2Id;

PRINT 'Sample data inserted successfully!';
PRINT '- 4 test users created (2 new speakers, 2 experienced speakers)';
PRINT '- User expertise assignments added';
PRINT '- Social media links added';
PRINT '- Sample mentorships created';
PRINT '';
PRINT 'Test user credentials (password for all: Password123!):';
PRINT '- jane.doe@example.com (New Speaker)';
PRINT '- alex.johnson@example.com (New Speaker)';
PRINT '- john.smith@example.com (Experienced Speaker)';
PRINT '- sarah.chen@example.com (Experienced Speaker)';
*/

-- =============================================
-- Database Health Check
-- =============================================
PRINT 'Database schema created successfully!';
PRINT 'Tables created: ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') AS VARCHAR(10));
PRINT 'Views created: ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS) AS VARCHAR(10));
PRINT 'Procedures created: ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE') AS VARCHAR(10));
PRINT 'Functions created: ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION') AS VARCHAR(10));

-- Verify table relationships
SELECT
    TABLE_NAME,
    CONSTRAINT_NAME,
    CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE IN ('PRIMARY KEY', 'FOREIGN KEY')
ORDER BY TABLE_NAME, CONSTRAINT_TYPE;

GO