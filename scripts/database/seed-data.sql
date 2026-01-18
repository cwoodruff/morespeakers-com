SET IDENTITY_INSERT MoreSpeakers.dbo.SpeakerTypes ON;
INSERT INTO MoreSpeakers.dbo.SpeakerTypes (Id, Name, Description, CreatedDate) VALUES (1, N'NewSpeaker', N'Aspiring speakers seeking mentorship and guidance', N'2025-09-20 21:36:21.2800000');
INSERT INTO MoreSpeakers.dbo.SpeakerTypes (Id, Name, Description, CreatedDate) VALUES (2, N'ExperiencedSpeaker', N'Experienced speakers offering mentorship', N'2025-09-20 21:36:21.2800000');
SET IDENTITY_INSERT MoreSpeakers.dbo.SpeakerTypes OFF;

INSERT INTO MoreSpeakers.dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (N'E2D256A3-3EB4-4037-B964-1CF4E31A7885', N'NewSpeaker', N'NEWSPEAKER', N'C3441EA9-EBDB-41D8-A85B-E331289A7914');
INSERT INTO MoreSpeakers.dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (N'2802E965-ECBE-4814-9597-4B5D9D86DC86', N'Administrator', N'ADMINISTRATOR', N'84CE6211-7FEE-48FB-9508-F77FDE0EEAF7');
INSERT INTO MoreSpeakers.dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (N'E808BC21-52ED-4242-A2C5-A77B6E6CB50E', N'ExperiencedSpeaker', N'EXPERIENCEDSPEAKER', N'A6BB63DD-7AD8-4068-8398-73F6C2DAB95F');

INSERT INTO MoreSpeakers.dbo.AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, Bio, SessionizeUrl, HeadshotUrl, SpeakerTypeId, Goals, CreatedDate, UpdatedDate, IsActive, IsAvailableForMentoring, MaxMentees, MentorshipFocus) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', N'cwoodruff@live.com', N'CWOODRUFF@LIVE.COM', N'cwoodruff@live.com', N'CWOODRUFF@LIVE.COM', 0, N'AQAAAAIAAYagAAAAEEIXrlcIOOnsJVbTQxiYwul61oWeKDu0tgtSjSafmw7Obixv0lqEKbN2cwRS5LHNAA==', N'ALNPQ2W4HBXTUJZTCKGIGLD7GXWIUEXI', N'47ef0ef9-7551-4347-b1e0-eb477c65f671', N'6167246885', 0, 0, null, 1, 0, N'Chris', N'Woodruff', N'Chris Woodruff has been at the forefront of software development since before the first .COM boom, building a career that spans enterprise web development, cloud solutions, software analytics, and developer relations. As an Architect, he applies his deep technical expertise to tackle complex challenges, with a particular focus on API design and scalable architectures. He is recognized as a Microsoft MVP specializing in .NET and Web Development. Woody’s impact extends beyond his professional responsibilities; he is a dedicated mentor and educator, teaching courses that help individuals transition into tech careers. His passion for sharing knowledge has made him a sought-after speaker at international conferences, where he discusses topics such as database development, web APIs, and software architecture. He contributes to the developer community by co-hosting **The Breakpoint Show** podcast and creating content that aids engineers in refining their skills. Previously, Woody led engineering teams at Rocket Homes, developed event-driven integration platforms, and spearheaded developer relations initiatives at Rocket Mortgage. His experience also includes serving as a Developer Advocate at JetBrains and architecting cloud-based analytics platforms at Eidex. Through his consulting work, he has assisted major companies, including Microsoft and MLB Advanced Media, in building robust software solutions. Beyond technology, Woody is an avid bourbon enthusiast, often exploring the Bourbon Trail in search of unique selections to share with friends. He also enjoys writing about his experiences in tech and life on his blog at https://woodruff.dev. You can stay connected with him on Bluesky at @woodruff.dev and on Mastodon at mastodon.social/@cwoodruff, where he engages with the developer community and shares insights on software, mentorship, and personal interests.', N'https://sessionize.com/chris-woodruff/', N'https://sessionize.com/image/76ba-400o400o2-S8SE8XWVTvxPaJzZPhJ5Bo.jpg', 2, N'To get more speakers!!', N'2025-09-28 16:13:18.1436792', N'2025-09-28 16:13:18.1436792', 1, 1, 2, null);
INSERT INTO MoreSpeakers.dbo.AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, Bio, SessionizeUrl, HeadshotUrl, SpeakerTypeId, Goals, CreatedDate, UpdatedDate, IsActive, IsAvailableForMentoring, MaxMentees, MentorshipFocus) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', N'glenn@henriksen.no', N'GLENN@HENRIKSEN.NO', N'glenn@henriksen.no', N'GLENN@HENRIKSEN.NO', 0, N'AQAAAAIAAYagAAAAECR1Sm+NEQ06Co/x2zBRoChx6gx7a07ikur3+vYWn39pM5YBL2jqvOCczH/G3O7zSg==', N'NOFZEWFWWMCMLTRV2CXK6C3Q4DLS33JB', N'3d0975ab-7502-4b66-b612-6b17a45557f1', N'+4790113235', 0, 0, null, 1, 0, N'Glenn', N'Henriksen', N'Glenn F. Henriksen is a mentor and developer from Norway. As the co-founder and CTO of Justify, he gets to build new legal tools for everyone to use, helping to create better communication and less conflict in relationships. He''s continuously exploring new tools, processes and technologies, and improving how he and his fellow developers work with code, tasks and projects. He has been a Microsoft Development MVP, a part of the Microsoft Regional Director program and is an ASP.NET Insider and an Azure Advisor. In the past 25+ years he has co-owned two companies, worked as a consultant, manager, support tech, network admin, developer, architect, technical lead and more, but his favorite things are still swapping code for food and building stuff that makes a difference in people’s lives.', N'https://sessionize.com/henriksen', N'https://cache.sessionize.com/image/1f97-400o400o2-3TSd5i8zsqRafnwHtY5GRX.jpg', 2, N'To get more people interested in speaking and lift new speakers up on the international stages', N'2025-09-28 14:18:56.0995355', N'2025-09-28 14:18:56.0995355', 1, 1, 2, null);
INSERT INTO MoreSpeakers.dbo.AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, Bio, SessionizeUrl, HeadshotUrl, SpeakerTypeId, Goals, CreatedDate, UpdatedDate, IsActive, IsAvailableForMentoring, MaxMentees, MentorshipFocus) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', N'jguadagno@hotmail.com', N'JGUADAGNO@HOTMAIL.COM', N'jguadagno@hotmail.com', N'JGUADAGNO@HOTMAIL.COM', 0, N'AQAAAAIAAYagAAAAEJnn2+kQwNcOHzPjVi93jnL94jngGzQXT9+eCQAmNGCXXJ3kSuIPI0XIi749/N88vQ==', N'3XLBD4DFP4NRRWYFFXQFEMDEFKCOTEDV', N'c2126868-5b47-44b2-9e36-a5666c8eda7a', N'16022936767', 0, 0, null, 1, 0, N'Joseph', N'Guadagno', N'I am groot', null, null, 1, N'To be a tree', N'2025-11-09 14:32:11.0779067', N'2025-11-09 14:32:11.0779069', 1, 1, 2, null);

SET IDENTITY_INSERT MoreSpeakers.dbo.Sectors ON;
INSERT INTO MoreSpeakers.dbo.Sectors (Id, Name, Slug, Description, DisplayOrder, IsActive)
VALUES (1, 'Technology', 'technology', 'Technology sector', 1, 1);
SET IDENTITY_INSERT MoreSpeakers.dbo.Sectors OFF;

SET IDENTITY_INSERT MoreSpeakers.dbo.ExpertiseCategories ON;
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (1	,'General','Default category for existing expertise',N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (2	,'.NET','.NET related languages',N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (3	,'Web Development','General web development frameworks', N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (4	,'Cloud','Cloud providers and technologies', N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (5	,'Programming Languages',	'Non .NET programming languages', N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (6	,'DevOps','Deployment and Container technologies', N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (7	,'AI','Artifical Intelligence and Machine Learning', N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (8	,'Databases','Database and Caching technologies', N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (9	,'Software Development','General topics around software development', N'2025-12-09 01:41:18.2917209', 1,1);
insert into MoreSpeakers.dbo.ExpertiseCategories (Id, Name, Description, CreatedDate, IsActive, SectorId) values (10	,'Leadership','General topics around leadership',N'2025-12-09 01:41:18.2917209', 1,1);
SET IDENTITY_INSERT MoreSpeakers.dbo.ExpertiseCategories OFF;

SET IDENTITY_INSERT MoreSpeakers.dbo.Expertises ON;
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (1, N'C#', N'C# programming language and .NET ecosystem', N'2025-09-20 21:36:21.2866667', 1, 2);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (2, N'.NET', N'.NET framework and .NET Core development', N'2025-09-20 21:36:21.2866667', 1, 2);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (3, N'ASP.NET Core', N'ASP.NET Core web application development', N'2025-09-20 21:36:21.2866667', 1, 3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (4, N'Blazor', N'Blazor web development framework', N'2025-09-20 21:36:21.2866667', 1, 3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (5, N'Azure', N'Microsoft Azure cloud platform and services', N'2025-09-20 21:36:21.2866667', 1, 4);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (6, N'JavaScript', N'JavaScript programming and web development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (7, N'TypeScript', N'TypeScript language and development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (8, N'React', N'React.js frontend development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (9, N'Angular', N'Angular framework development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (10, N'Vue.js', N'Vue.js frontend framework', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (11, N'Node.js', N'Node.js backend development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (12, N'Python', N'Python programming language', N'2025-09-20 21:36:21.2866667', 1,5);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (13, N'Java', N'Java programming language and ecosystem', N'2025-09-20 21:36:21.2866667', 1,5);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (14, N'Kubernetes', N'Kubernetes container orchestration', N'2025-09-20 21:36:21.2866667', 1,4);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (15, N'Docker', N'Docker containerization technology', N'2025-09-20 21:36:21.2866667', 1,4 );
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (16, N'DevOps', N'DevOps practices and methodologies', N'2025-09-20 21:36:21.2866667', 1,6);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (17, N'CI/CD', N'Continuous Integration and Continuous Deployment', N'2025-09-20 21:36:21.2866667', 1,6);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (18, N'Machine Learning', N'Machine Learning and AI development', N'2025-09-20 21:36:21.2866667', 1,7);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (19, N'Data Science', N'Data Science and analytics', N'2025-09-20 21:36:21.2866667', 1,7);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (20, N'SQL Server', N'Microsoft SQL Server database', N'2025-09-20 21:36:21.2866667', 1,8);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (21, N'PostgreSQL', N'PostgreSQL database development', N'2025-09-20 21:36:21.2866667', 1,8);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (22, N'MongoDB', N'MongoDB NoSQL database', N'2025-09-20 21:36:21.2866667', 1,8);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (23, N'Redis', N'Redis caching and data store', N'2025-09-20 21:36:21.2866667', 1,8);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (24, N'Microservices', N'Microservices architecture and development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (25, N'API Design', N'API design and development best practices', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (26, N'GraphQL', N'GraphQL API development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (27, N'REST', N'RESTful API design and development', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (28, N'Security', N'Application and system security', N'2025-09-20 21:36:21.2866667', 1,3);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (29, N'Performance Optimization', N'Application performance tuning', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (30, N'Testing', N'Software testing methodologies', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (31, N'TDD', N'Test-Driven Development', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (32, N'BDD', N'Behavior-Driven Development', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (33, N'Agile', N'Agile development methodologies', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (34, N'Scrum', N'Scrum framework and practices', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (35, N'Leadership', N'Technical leadership and management', N'2025-09-20 21:36:21.2866667', 1,10);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (36, N'Architecture', N'Software architecture and design', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (37, N'Design Patterns', N'Software design patterns', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (38, N'Clean Code', N'Clean code principles and practices', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (39, N'SOLID Principles', N'SOLID design principles', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (40, N'Domain-Driven Design', N'Domain-Driven Design methodology', N'2025-09-20 21:36:21.2866667', 1,9);
INSERT INTO MoreSpeakers.dbo.Expertises (Id, Name, Description, CreatedDate, IsActive, ExpertiseCategoryId) VALUES (41, N'htmx', N'Custom expertise: htmx', N'2025-09-25 19:58:23.1483350', 1,3);
SET IDENTITY_INSERT MoreSpeakers.dbo.Expertises OFF;

INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 1, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 2, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 3, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 5, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 6, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 20, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 25, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 27, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 35, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 36, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 37, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 41, N'2025-09-28 16:13:18.4300000');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 1, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 2, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 3, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 5, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 8, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 16, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 17, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 20, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 25, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 26, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 30, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 31, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 33, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 34, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 36, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 37, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 38, N'2025-09-28 14:18:56.6733333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3BF69400-E4E2-446C-88BA-BD89BDF17127', 39, N'2025-09-28 14:18:56.6733333');INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'3cae1cf9-c6a8-4b50-91e5-9030a678a0fa', 1, N'2025-11-09 14:32:09.7433333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 2, N'2025-11-09 14:32:09.7433333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 3, N'2025-11-09 14:32:09.7433333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 33, N'2025-11-09 14:32:09.7433333');
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 36, N'2025-11-09 14:32:09.7433333');

SET IDENTITY_INSERT MoreSpeakers.dbo.SocialMediaSites ON;
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (1, 'Bluesky', 'bi bi-bluesky', 'https://bsky.app/profile/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (2, 'Discord', 'bi bi-discord', 'https://discord.com/users/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (3, 'Facebook', 'bi bi-facebook', 'https://www.facebook.com/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (4, 'GitHub', 'bi bi-github', 'https://www.github.com/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (5, 'Instagram', 'bi bi-instagram', 'https://www.instagram.com/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (6, 'LinkedIn', 'bi bi-linkedin', 'https://www.linkedin.com/in/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (7, 'Mastodon', 'bi bi-mastodon', '{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (8, 'Medium', 'bi bi-medium', 'https://www.medium.com/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (9, 'PayPal', 'bi bi-paypal', 'https://www.paypal.me/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (10, 'Reddit', 'bi bi-reddit', 'https://www.reddit.com/u/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (11, 'Signal', 'bi bi-signal', 'https://signal.me/#p/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (12, 'StackOverflow', 'bi bi-stack-overflow', 'https://www.stackoverflow.com/users/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (13, 'Twitch', 'bi bi-twitch', 'https://www.twitch.tv/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (14, 'Twitter/X', 'bi bi-twitter-x', 'https://www.x.com/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (15, 'Youtube', 'bi bi-youtube', 'https://www.youtube.com/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (16, 'Website', 'bi bi-globe', '{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (17, 'Other', 'bi bi-link', '{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (18, 'Blog', 'bi bi-journal-text', '{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (19, 'WhatsApp', 'bi bi-whatsapp', 'https://wa.me/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (20, 'Substack', 'bi bi-substack', 'https://substack.com/{0}');
INSERT INTO MoreSpeakers.dbo.SocialMediaSites (Id, Name, Icon, UrlFormat) VALUES (21, 'Hacker News', 'fa-brands fa-hacker-news', 'https://news.ycombinator.com/user?id={0}');
SET IDENTITY_INSERT MoreSpeakers.dbo.SocialMediaSites OFF;

INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 18, N'https://www.woodruff.dev/category/blog/');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 16, N'https://www.woodruff.dev/');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 6, N'chriswoodruff');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'62A49FAC-ECD3-432F-8B92-7C558ACE9575', 4, N'cwoodruff');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'3bf69400-e4e2-446c-88ba-bd89bdf17127', 16, N'https://www.henriksen.no');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'3bf69400-e4e2-446c-88ba-bd89bdf17127', 6, N'glennhenriksen');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 1, N'jguadagno.com');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 3, N'JosephGuadagnoNet');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 4, N'jguadagno');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 6, N'josephguadagno');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 7, N'https://techhub.social/@Jguadagno');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 9, N'jguadagno');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 12, N'89184');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 13, N'jguadagno');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 14, N'jguadagno');
INSERT INTO MoreSpeakers.dbo.UserSocialMediaSites (UserId, SocialMediaSiteId, SocialId) VALUES (N'E6F76213-C3E1-40B4-9E24-97D69D177855', 18, N'https://josephguadagno.net/');

-- GENERATE 35 USERS FOR PAGINATION TESTING
INSERT INTO MoreSpeakers.dbo.AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, Bio, SpeakerTypeId, Goals, CreatedDate, UpdatedDate, IsActive,
    IsAvailableForMentoring, MaxMentees
)
SELECT TOP 35
    NEWID(),
    'speaker' + CAST(ROW_NUMBER() OVER (ORDER BY a.object_id) AS NVARCHAR(10)) + '@test.com',
       'SPEAKER' + CAST(ROW_NUMBER() OVER (ORDER BY a.object_id) AS NVARCHAR(10)) + '@TEST.COM',
       'speaker' + CAST(ROW_NUMBER() OVER (ORDER BY a.object_id) AS NVARCHAR(10)) + '@test.com',
       'SPEAKER' + CAST(ROW_NUMBER() OVER (ORDER BY a.object_id) AS NVARCHAR(10)) + '@TEST.COM',
       1,
       'AQAAAAIAAYagAAAAEEIXrlcIOOnsJVbTQxiYwul61oWeKDu0tgtSjSafmw7Obixv0lqEKbN2cwRS5LHNAA==', -- Same hash as seeded users
       NEWID(), NEWID(),
       '1-555-555-' + RIGHT('0000' + CAST(ROW_NUMBER() OVER (ORDER BY a.object_id) AS NVARCHAR(4)), 4), -- Generates 1-555-555-0001 (14 chars)
    1, 0, 1, 0,
    'TestUser', -- FirstName (Length > 2)
    'Speaker ' + CAST(ROW_NUMBER() OVER (ORDER BY a.object_id) AS NVARCHAR(10)), -- LastName (Length > 2 guaranteed)
    'Generated bio for pagination testing. User ' + CAST(ROW_NUMBER() OVER (ORDER BY a.object_id) AS NVARCHAR(10)),
    (ROW_NUMBER() OVER (ORDER BY a.object_id) % 2) + 1, -- Alternates between 1 (New) and 2 (Experienced)
    'Testing goals',
    GETUTCDATE(), GETUTCDATE(), 1, 1, 2
FROM sys.all_columns a;

-- Give them all C# expertise so they show up in default searches
INSERT INTO MoreSpeakers.dbo.UserExpertises (UserId, ExpertiseId, CreatedDate)
SELECT Id, 1, GETUTCDATE()
FROM MoreSpeakers.dbo.AspNetUsers
WHERE Email LIKE 'speaker%@test.com';

-- Adding needed roles into AspNetRoles
NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'USERMANAGER')
    INSERT INTO AspNetRoles (Id, [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), N'UserManager', N'USERMANAGER', CONVERT(nvarchar(128), NEWID()));

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'CATALOGMANAGER')
    INSERT INTO AspNetRoles (Id, [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), N'CatalogManager', N'CATALOGMANAGER', CONVERT(nvarchar(128), NEWID()));

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'REPORTER')
    INSERT INTO AspNetRoles (Id, [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), N'Reporter', N'REPORTER', CONVERT(nvarchar(128), NEWID()));

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'MODERATOR')
    INSERT INTO AspNetRoles (Id, [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), N'Moderator', N'MODERATOR', CONVERT(nvarchar(128), NEWID()));


INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/Pages/Shared/_EmailLayout.cshtml',
    N'<!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8">
        <title></title>
        <style>
            body { font-family: ''Inter'', Arial, sans-serif; line-height: 1.6; color: #333; }
            .container { max-width: 600px; margin: 0 auto; padding: 20px; }
            .header { background: linear-gradient(135deg, #fd7e14 0%, #e55d0e 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }
            .content { background: white; padding: 30px; border-radius: 0 0 8px 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
            .badge { background: #fd7e14; color: white; padding: 8px 16px; border-radius: 20px; display: inline-block; font-size: 14px; font-weight: bold; }
            .next-steps { background: #f8f9fa; padding: 20px; border-radius: 6px; margin: 20px 0; }
            .step { display: flex; align-items: center; margin: 15px 0; }
            .step-number { background: #fd7e14; color: white; width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px; font-weight: bold; }
            .btn { background: #fd7e14; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold; }
            .footer { text-align: center; margin-top: 30px; color: #666; font-size: 14px; }
        </style>
    </head>
    <body>
        <div class="container">
            <div class="header">
                <h1>🎤 Welcome from MoreSpeakers.com!</h1>
                <p>Your speaking journey starts here</p>
            </div>

            <div class="content">
                @RenderBody()
            </div>

            <h3>🤝 Community Guidelines</h3>
            <p>We''re building a supportive and inclusive community. Please be respectful, helpful, and authentic in all your interactions.</p>

            <div class="footer">
                <p>
                    <strong>MoreSpeakers.com</strong><br>
                    Connecting speakers, sharing knowledge, building community<br>
                    <a href="mailto:support@morespeakers.com">support@morespeakers.com</a>
                </p>
                <p style="font-size: 12px; color: #999;">
                    You received this email because you registered an account at MoreSpeakers.com.<br>
                    If you have any questions, please contact our support team.
                </p>
            </div>
        </div>
    </body>
    </html>"'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/ConfirmUserEmail.cshtml',
    N'@model MoreSpeakers.Domain.Models.UserConfirmationEmail

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.User.FullName!</h2>
<p>Thank you for registering with MoreSpeakers.com. We''re excited to have you on board.</p>

Now that you registered for an account, you can start sharing your knowledge and experiences with the world.
In order to take the next step in your journey, please confirm your email address.  We promise we won''t spam you.

<a href="@Model.ConfirmationUrl" class="btn">Confirm Email</a>
'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestAcceptedFromMentee.cshtml',
    '@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.Mentor.FirstName!</h2>

<p>Congratulations your mentoring request was accepted from @Model.Mentor.FullName! </p>

<p>We''re thrilled to have you as a mentee now in our growing network of passionate speakers.</p>

<p>You should contact @Model.Mentor.FirstName to schedule your first meeting based on the @Model.PreferredFrequency frequency.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestAcceptedToMentor.cshtml',
    '@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.Mentor.FirstName!</h2>

<p>Congratulations on accepting the mentoring request from @Model.Mentee.FullName!</p>

<p>We''re thrilled to have you as a mentor in our growing network of passionate speakers.</p>

<p>You should contact @Model.Mentee.FirstName to schedule your first meeting based on the @Model.PreferredFrequency frequency. Your new mentee contact information is:</p>

<p>
Email - @Model.Mentee.Email
Phone Number - @Model.Mentee.PhoneNumber
</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestCancelledFromMentee.cshtml',
    '@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that the mentorship has been canceled.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestCancelledToMentor.cshtml',
    '@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that the mentorship has been canceled.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestDeclinedFromMentee.cshtml',
    N'@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentor.FullName, </p>

<p>We wanted to let you know that @Model.Mentor.FullName declined the mentorship request from you. We understand that this news may not be the best at this time but just know that there are many great mentors at MoeSpeakers.com.</p>

<p>Please keep looking at the list of possible mentors. We know there is a great match for you coming soon.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestDeclinedToMentor.cshtml',
    N'@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that the mentorship with @Model.Mentee.FullName has been declined by yourself. We know it was a hard decision and we hope that you find a fulfilling mentoring connection soon with another new speaker in the MoreSpeakers.com community. </p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestFromMentee.cshtml',
    '@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that we received your mentorship request. The request has been sent to @Model.Mentor.FullName.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>
'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/MentorshipRequestToMentor.cshtml',
    '@model MoreSpeakers.Domain.Models.Mentorship

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentor.FullName, </p>

<p>We wanted to let you know that @Model.Mentee.FullName has sent a request to you for mentorship. Please review the request at MoreSpeakers.com and respond at your earliest convenience.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/PasswordReset.cshtml',
    '@model MoreSpeakers.Domain.Models.UserPasswordResetEmail;

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Reset Your Password</h2>
<p>Hello @Model.User.FullName,</p>

<p>We received a request to reset the password for your MoreSpeakers.com account. If you didn''t make this request, you can safely ignore this email.</p>

<p>To reset your password, please click the button below:</p>

<p>
<a href="@Model.ResetEmailUrl" class="btn">Reset Password</a>
</p>

<p>For security reasons, this link will expire in 24 hours.</p>

<p>If you''re having trouble clicking the "Reset Password" button, copy and paste the URL below into your web browser:</p>
<p>@Model.ResetEmailUrl</p>

<p>Thanks,<br />
The MoreSpeakers Team</p>
'
    );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
    '/EmailTemplates/WelcomeEmail.cshtml',
    N'@model MoreSpeakers.Domain.Models.User

@{
Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.FirstName!</h2>

<p>Congratulations on joining the MoreSpeakers.com community! We''re thrilled to have you as a <span class="badge">@Model.SpeakerType.ToFriendlyName()</span> in our growing network of passionate speakers.</p>

<h3>Your Registration Details:</h3>
<ul>
<li><strong>Name:</strong> @Model.FirstName @Model.LastName</li>
<li><strong>Email:</strong> @Model.Email</li>
<li><strong>Speaker Type:</strong> @Model.SpeakerType.ToFriendlyName()</li>
<li><strong>Registration Date:</strong> @(DateTime.UtcNow.ToString("MMMM dd, yyyy"))</li>
</ul>

<div class="next-steps">
<h3>🚀 What''s Next?</h3>

<div class="step">
 <div class="step-number">1</div>
 <div>
     <strong>Complete Your Profile</strong><br>
     <small>Add more details, upload a headshot, and showcase your expertise to help others find and connect with you.</small>
 </div>
</div>

<div class="step">
 <div class="step-number">2</div>
 <div>
     <strong>@(Model.SpeakerTypeId == (int) MoreSpeakers.Domain.Models.SpeakerTypeEnum.NewSpeaker ? "Find Mentors" : "Connect with New Speakers")</strong><br>
     <small>@(Model.SpeakerTypeId == (int) MoreSpeakers.Domain.Models.SpeakerTypeEnum.NewSpeaker)
         ? "Browse our community of experienced speakers and find mentors who can guide your speaking journey."
         : "Discover new speakers who could benefit from your experience and mentorship.")</small>
 </div>
</div>

<div class="step">
 <div class="step-number">3</div>
 <div>
     <strong>Join the Community</strong><br>
     <small>Participate in discussions, share your experiences, and connect with fellow speakers from around the world.</small>
 </div>
</div>
</div>

<div style="text-align: center; margin: 30px 0;">
<a href="https://www.morespeakers.com/Profile/Edit" class="btn">Complete My Profile →</a>
</div>

<h3>📧 Email Confirmation Required</h3>
<p>To activate your account and access all features, please check your email for a confirmation link. If you don''t see it in your inbox, don''t forget to check your spam folder.</p>
'
    );
