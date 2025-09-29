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
-- Create Functions
-- =============================================

-- Function to calculate user's expertise match percentage
CREATE FUNCTION [dbo].[fn_CalculateExpertiseMatch]
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
/****** Object:  Table [dbo].[Mentorships]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mentorships](
	[Id] [uniqueidentifier] NOT NULL,
	[MentorId] [uniqueidentifier] NOT NULL,
	[MenteeId] [uniqueidentifier] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[RequestMessage] [nvarchar](1000) NOT NULL,
	[ResponseMessage] [nvarchar](1000) NULL,
	[PreferredFrequency] [nvarchar](50) NULL,
	[RequestedAt] [datetime2](7) NOT NULL,
	[ResponsedAt] [datetime2](7) NULL,
	[StartedAt] [datetime2](7) NULL,
	[CompletedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[Notes] [nvarchar](2000) NULL,
	[NewSpeakerId]  AS ([MenteeId]) PERSISTED NOT NULL,
	[RequestDate]  AS ([RequestedAt]) PERSISTED NOT NULL,
	[AcceptedDate]  AS ([StartedAt]) PERSISTED,
 CONSTRAINT [PK_Mentorships] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SpeakerTypes]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpeakerTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [uniqueidentifier] NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Bio] [nvarchar](max) NOT NULL,
	[SessionizeUrl] [nvarchar](500) NULL,
	[HeadshotUrl] [nvarchar](500) NULL,
	[SpeakerTypeId] [int] NOT NULL,
	[Goals] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsAvailableForMentoring] [bit] NOT NULL,
	[MaxMentees] [int] NOT NULL,
	[MentorshipFocus] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserExpertise]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserExpertise](
	[UserId] [uniqueidentifier] NOT NULL,
	[ExpertiseId] [int] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ExpertiseId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SocialMedia]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SocialMedia](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Platform] [nvarchar](50) NOT NULL,
	[Url] [nvarchar](500) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_SpeakerDetails]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Create Views for Common Queries
-- =============================================

-- View for speaker details with expertise
CREATE VIEW [dbo].[vw_SpeakerDetails] AS
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
/****** Object:  View [dbo].[vw_ActiveMentorships]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- View for active mentorships with speaker details
CREATE VIEW [dbo].[vw_ActiveMentorships] AS
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
/****** Object:  Table [dbo].[Expertise]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Expertise](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_ExpertisePopularity]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- View for expertise popularity
CREATE VIEW [dbo].[vw_ExpertisePopularity] AS
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
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [uniqueidentifier] NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [uniqueidentifier] NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MentorshipExpertise]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MentorshipExpertise](
	[MentorshipId] [uniqueidentifier] NOT NULL,
	[ExpertiseId] [int] NOT NULL,
 CONSTRAINT [PK_MentorshipExpertise] PRIMARY KEY CLUSTERED 
(
	[MentorshipId] ASC,
	[ExpertiseId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_MentorshipExpertise_ExpertiseId]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_MentorshipExpertise_ExpertiseId] ON [dbo].[MentorshipExpertise]
(
	[ExpertiseId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Mentorships_MenteeId]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_Mentorships_MenteeId] ON [dbo].[Mentorships]
(
	[MenteeId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Mentorships_MenteeId_Status]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_Mentorships_MenteeId_Status] ON [dbo].[Mentorships]
(
	[MenteeId] ASC,
	[Status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Mentorships_MentorId]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_Mentorships_MentorId] ON [dbo].[Mentorships]
(
	[MentorId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Mentorships_MentorId_Status]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_Mentorships_MentorId_Status] ON [dbo].[Mentorships]
(
	[MentorId] ASC,
	[Status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Mentorships_Status]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_Mentorships_Status] ON [dbo].[Mentorships]
(
	[Status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Mentorships_Status_RequestedAt]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_Mentorships_Status_RequestedAt] ON [dbo].[Mentorships]
(
	[Status] ASC,
	[RequestedAt] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Mentorships_Type]    Script Date: 9/29/2025 8:59:30 AM ******/
CREATE NONCLUSTERED INDEX [IX_Mentorships_Type] ON [dbo].[Mentorships]
(
	[Type] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetRoles] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((0)) FOR [EmailConfirmed]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((0)) FOR [PhoneNumberConfirmed]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((0)) FOR [TwoFactorEnabled]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((0)) FOR [LockoutEnabled]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((0)) FOR [AccessFailedCount]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT (getutcdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((1)) FOR [IsAvailableForMentoring]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ((2)) FOR [MaxMentees]
GO
ALTER TABLE [dbo].[Expertise] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Expertise] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Mentorships] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Mentorships] ADD  DEFAULT ('NewToExperienced') FOR [Type]
GO
ALTER TABLE [dbo].[Mentorships] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[Mentorships] ADD  DEFAULT ('') FOR [RequestMessage]
GO
ALTER TABLE [dbo].[Mentorships] ADD  DEFAULT (getutcdate()) FOR [RequestedAt]
GO
ALTER TABLE [dbo].[Mentorships] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Mentorships] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[SocialMedia] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[SocialMedia] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[SpeakerTypes] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[UserExpertise] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUsers]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUsers_SpeakerTypes] FOREIGN KEY([SpeakerTypeId])
REFERENCES [dbo].[SpeakerTypes] ([Id])
GO
ALTER TABLE [dbo].[AspNetUsers] CHECK CONSTRAINT [FK_AspNetUsers_SpeakerTypes]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[MentorshipExpertise]  WITH CHECK ADD  CONSTRAINT [FK_MentorshipExpertise_Expertise_ExpertiseId] FOREIGN KEY([ExpertiseId])
REFERENCES [dbo].[Expertise] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MentorshipExpertise] CHECK CONSTRAINT [FK_MentorshipExpertise_Expertise_ExpertiseId]
GO
ALTER TABLE [dbo].[MentorshipExpertise]  WITH CHECK ADD  CONSTRAINT [FK_MentorshipExpertise_Mentorships_MentorshipId] FOREIGN KEY([MentorshipId])
REFERENCES [dbo].[Mentorships] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MentorshipExpertise] CHECK CONSTRAINT [FK_MentorshipExpertise_Mentorships_MentorshipId]
GO
ALTER TABLE [dbo].[Mentorships]  WITH CHECK ADD  CONSTRAINT [FK_Mentorships_AspNetUsers_MenteeId] FOREIGN KEY([MenteeId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[Mentorships] CHECK CONSTRAINT [FK_Mentorships_AspNetUsers_MenteeId]
GO
ALTER TABLE [dbo].[Mentorships]  WITH CHECK ADD  CONSTRAINT [FK_Mentorships_AspNetUsers_MentorId] FOREIGN KEY([MentorId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[Mentorships] CHECK CONSTRAINT [FK_Mentorships_AspNetUsers_MentorId]
GO
ALTER TABLE [dbo].[SocialMedia]  WITH CHECK ADD  CONSTRAINT [FK_SocialMedia_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SocialMedia] CHECK CONSTRAINT [FK_SocialMedia_Users]
GO
ALTER TABLE [dbo].[UserExpertise]  WITH CHECK ADD  CONSTRAINT [FK_UserExpertise_Expertise] FOREIGN KEY([ExpertiseId])
REFERENCES [dbo].[Expertise] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserExpertise] CHECK CONSTRAINT [FK_UserExpertise_Expertise]
GO
ALTER TABLE [dbo].[UserExpertise]  WITH CHECK ADD  CONSTRAINT [FK_UserExpertise_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserExpertise] CHECK CONSTRAINT [FK_UserExpertise_Users]
GO
ALTER TABLE [dbo].[AspNetUsers]  WITH CHECK ADD  CONSTRAINT [CHK_Users_Bio_Length] CHECK  ((len([Bio])<=(6000)))
GO
ALTER TABLE [dbo].[AspNetUsers] CHECK CONSTRAINT [CHK_Users_Bio_Length]
GO
ALTER TABLE [dbo].[AspNetUsers]  WITH CHECK ADD  CONSTRAINT [CHK_Users_FirstName_Length] CHECK  ((len([FirstName])>=(2) AND len([FirstName])<=(100)))
GO
ALTER TABLE [dbo].[AspNetUsers] CHECK CONSTRAINT [CHK_Users_FirstName_Length]
GO
ALTER TABLE [dbo].[AspNetUsers]  WITH CHECK ADD  CONSTRAINT [CHK_Users_Goals_Length] CHECK  ((len([Goals])<=(2000)))
GO
ALTER TABLE [dbo].[AspNetUsers] CHECK CONSTRAINT [CHK_Users_Goals_Length]
GO
ALTER TABLE [dbo].[AspNetUsers]  WITH CHECK ADD  CONSTRAINT [CHK_Users_LastName_Length] CHECK  ((len([LastName])>=(2) AND len([LastName])<=(100)))
GO
ALTER TABLE [dbo].[AspNetUsers] CHECK CONSTRAINT [CHK_Users_LastName_Length]
GO
ALTER TABLE [dbo].[AspNetUsers]  WITH CHECK ADD  CONSTRAINT [CHK_Users_Phone_Format] CHECK  (([PhoneNumber] IS NULL OR len([PhoneNumber])>=(10) AND len([PhoneNumber])<=(20) AND [PhoneNumber] like '%[0-9]%'))
GO
ALTER TABLE [dbo].[AspNetUsers] CHECK CONSTRAINT [CHK_Users_Phone_Format]
GO
ALTER TABLE [dbo].[AspNetUsers]  WITH CHECK ADD  CONSTRAINT [CHK_Users_SessionizeUrl_Format] CHECK  (([SessionizeUrl] IS NULL OR [SessionizeUrl] like 'http%://%' OR [SessionizeUrl] like 'https%://%'))
GO
ALTER TABLE [dbo].[AspNetUsers] CHECK CONSTRAINT [CHK_Users_SessionizeUrl_Format]
GO
ALTER TABLE [dbo].[Mentorships]  WITH CHECK ADD  CONSTRAINT [CHK_Mentorships_DifferentUsers] CHECK  (([MenteeId]<>[MentorId]))
GO
ALTER TABLE [dbo].[Mentorships] CHECK CONSTRAINT [CHK_Mentorships_DifferentUsers]
GO
ALTER TABLE [dbo].[Mentorships]  WITH CHECK ADD  CONSTRAINT [CHK_Mentorships_Status] CHECK  (([Status]='Cancelled' OR [Status]='Declined' OR [Status]='Completed' OR [Status]='Active' OR [Status]='Pending'))
GO
ALTER TABLE [dbo].[Mentorships] CHECK CONSTRAINT [CHK_Mentorships_Status]
GO
ALTER TABLE [dbo].[Mentorships]  WITH CHECK ADD  CONSTRAINT [CHK_Mentorships_Type] CHECK  (([Type]='ExperiencedToExperienced' OR [Type]='NewToExperienced'))
GO
ALTER TABLE [dbo].[Mentorships] CHECK CONSTRAINT [CHK_Mentorships_Type]
GO
ALTER TABLE [dbo].[SocialMedia]  WITH CHECK ADD  CONSTRAINT [CHK_SocialMedia_Url_Format] CHECK  (([Url] like 'http%://%' OR [Url] like 'https%://%'))
GO
ALTER TABLE [dbo].[SocialMedia] CHECK CONSTRAINT [CHK_SocialMedia_Url_Format]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetMentorshipStatistics]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Procedure to get mentorship statistics
CREATE PROCEDURE [dbo].[sp_GetMentorshipStatistics]
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
/****** Object:  StoredProcedure [dbo].[sp_GetSpeakerRecommendations]    Script Date: 9/29/2025 8:59:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Create Stored Procedures
-- =============================================

-- Procedure to get speaker recommendations
CREATE PROCEDURE [dbo].[sp_GetSpeakerRecommendations]
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
    GROUP BY s.Id, s.FirstName, s.LastName, s.Email, s.Bio, s.HeadshotUrl, s.SpeakerTypeId,  s.CreatedDate
    ORDER BY COUNT(DISTINCT ue2.ExpertiseId) DESC, s.CreatedDate DESC;
END;
GO
ALTER DATABASE [MoreSpeakers] SET  READ_WRITE 
GO
