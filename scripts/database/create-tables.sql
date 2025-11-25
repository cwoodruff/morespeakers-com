create table AspNetRoles
(
    Id               uniqueidentifier default newid() not null
        primary key,
    Name             nvarchar(256),
    NormalizedName   nvarchar(256),
    ConcurrencyStamp nvarchar(max)
)
go

create table AspNetRoleClaims
(
    Id         int identity
        primary key,
    RoleId     uniqueidentifier not null
        constraint FK_AspNetRoleClaims_AspNetRoles_RoleId
            references AspNetRoles
            on delete cascade,
    ClaimType  nvarchar(max),
    ClaimValue nvarchar(max)
)
go

create table SpeakerTypes
(
    Id          int identity
        primary key,
    Name        nvarchar(50)                   not null
        unique,
    Description nvarchar(200)                  not null,
    CreatedDate datetime2 default getutcdate() not null
)
go

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
go

create table AspNetUserClaims
(
    Id         int identity
        primary key,
    UserId     uniqueidentifier not null
        constraint FK_AspNetUserClaims_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    ClaimType  nvarchar(max),
    ClaimValue nvarchar(max)
)
go

create table AspNetUserLogins
(
    LoginProvider       nvarchar(450)    not null,
    ProviderKey         nvarchar(450)    not null,
    ProviderDisplayName nvarchar(max),
    UserId              uniqueidentifier not null
        constraint FK_AspNetUserLogins_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    primary key (LoginProvider, ProviderKey)
)
go

create table AspNetUserRoles
(
    UserId uniqueidentifier not null
        constraint FK_AspNetUserRoles_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    RoleId uniqueidentifier not null
        constraint FK_AspNetUserRoles_AspNetRoles_RoleId
            references AspNetRoles
            on delete cascade,
    primary key (UserId, RoleId)
)
go

create table AspNetUserTokens
(
    UserId        uniqueidentifier not null
        constraint FK_AspNetUserTokens_AspNetUsers_UserId
            references AspNetUsers
            on delete cascade,
    LoginProvider nvarchar(450)    not null,
    Name          nvarchar(450)    not null,
    Value         nvarchar(max),
    primary key (UserId, LoginProvider, Name)
)
go

create table Expertises
(
    Id          int identity
        primary key,
    Name        nvarchar(100)                  not null
        unique,
    Description nvarchar(500),
    CreatedDate datetime2 default getutcdate() not null,
    IsActive    bit       default 1            not null
)
go


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

create index IX_MentorshipExpertise_ExpertiseId
    on MentorshipExpertises (ExpertiseId)
go

create index IX_Mentorships_MentorId
    on Mentorships (MentorId)
go

create index IX_Mentorships_MenteeId
    on Mentorships (MenteeId)
go

create index IX_Mentorships_Status
    on Mentorships (Status)
go

create index IX_Mentorships_Type
    on Mentorships (Type)
go

create index IX_Mentorships_Status_RequestedAt
    on Mentorships (Status, RequestedAt)
go

create index IX_Mentorships_MentorId_Status
    on Mentorships (MentorId, Status)
go

create index IX_Mentorships_MenteeId_Status
    on Mentorships (MenteeId, Status)
go

create table UserExpertises
(
    UserId      uniqueidentifier               not null
        constraint FK_UserExpertises_Users
            references AspNetUsers
            on delete cascade,
    ExpertiseId int                            not null
        constraint FK_UserExpertises_Expertises
            references Expertises
            on delete cascade,
    CreatedDate datetime2 default getutcdate() not null,
    primary key (UserId, ExpertiseId)
)
go

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
