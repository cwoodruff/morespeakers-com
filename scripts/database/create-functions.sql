
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
    FROM UserExpertises ue1
             INNER JOIN UserExpertises ue2 ON ue1.ExpertiseId = ue2.ExpertiseId
    WHERE ue1.UserId = @UserId1 AND ue2.UserId = @UserId2;

    -- Count total unique expertise areas between both users
    SELECT @TotalUniqueCount = COUNT(DISTINCT ExpertiseId)
    FROM UserExpertises
    WHERE UserId IN (@UserId1, @UserId2);

    -- Calculate percentage
    IF @TotalUniqueCount > 0
        SET @MatchPercentage = (@CommonCount * 100.0) / @TotalUniqueCount;
    ELSE
        SET @MatchPercentage = 0;

    RETURN @MatchPercentage;
END;
go


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
go


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
             INNER JOIN UserExpertises ue2 ON s.Id = ue2.UserId
             INNER JOIN UserExpertises ue1 ON ue2.ExpertiseId = ue1.ExpertiseId
             LEFT JOIN UserExpertises ue3 ON s.Id = ue3.UserId
    WHERE ue1.UserId = @UserId
      AND s.Id != @UserId
      AND s.IsActive = 1
    GROUP BY s.Id, s.FirstName, s.LastName, s.Email, s.Bio, s.HeadshotUrl, s.SpeakerTypeId,  s.CreatedDate
    ORDER BY COUNT(DISTINCT ue2.ExpertiseId) DESC, s.CreatedDate DESC;
END;
go


