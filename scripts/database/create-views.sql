
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
go


-- View for expertise popularity
CREATE VIEW [dbo].[vw_ExpertisePopularity] AS
SELECT
    e.Id,
    e.Name,
    e.Description,
    COUNT(ue.UserId) AS UserCount,
    COUNT(CASE WHEN u.SpeakerTypeId = 1 THEN 1 END) AS NewSpeakerCount,
    COUNT(CASE WHEN u.SpeakerTypeId = 2 THEN 1 END) AS ExperiencedSpeakerCount
FROM Expertises e
         LEFT JOIN UserExpertises ue ON e.Id = ue.ExpertiseId
         LEFT JOIN AspNetUsers u ON ue.UserId = u.Id AND u.IsActive = 1
WHERE e.IsActive = 1
GROUP BY e.Id, e.Name, e.Description;
go


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
         LEFT JOIN UserExpertises ue ON u.Id = ue.UserId
         LEFT JOIN UserSocialMediaSites sm ON u.Id = sm.UserId
         LEFT JOIN Mentorships m1 ON u.Id = m1.MentorId
         LEFT JOIN Mentorships m2 ON u.Id = m2.NewSpeakerId
WHERE u.IsActive = 1
GROUP BY u.Id, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.Bio,
         u.SessionizeUrl, u.HeadshotUrl, u.Goals, u.CreatedDate, u.UpdatedDate,
         st.Name, st.Description;
go


