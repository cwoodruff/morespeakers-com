-- SQL Script to Delete a User from MoreSpeakers.com Database
-- This script safely removes a user and all their related data

-- Usage: Replace @UserId with the actual user ID (GUID) you want to delete
-- Example: DECLARE @UserId UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012';

DECLARE @UserId UNIQUEIDENTIFIER;

-- Set the user ID to delete (REPLACE THIS WITH ACTUAL USER ID)
SET @UserId = '505b4e55-df96-4865-ac0b-c349d533bb7e'; -- CHANGE THIS!

-- Verify user exists before deletion
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = @UserId)
BEGIN
    PRINT 'Error: User with ID ' + CAST(@UserId AS VARCHAR(50)) + ' does not exist.';
    RETURN;
END

-- Display user info before deletion (for confirmation)
SELECT 
    'User to be deleted:' as Action,
    Id,
    UserName,
    Email,
    FirstName,
    LastName,
    CreatedDate
FROM AspNetUsers 
WHERE Id = @UserId;

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Delete from AspNetUserRoles (user role assignments)
    DELETE FROM AspNetUserRoles WHERE UserId = @UserId;
    PRINT 'Deleted user roles';

    -- 2. Delete from AspNetUserClaims (user claims)
    DELETE FROM AspNetUserClaims WHERE UserId = @UserId;
    PRINT 'Deleted user claims';

    -- 3. Delete from AspNetUserLogins (external login providers)
    DELETE FROM AspNetUserLogins WHERE UserId = @UserId;
    PRINT 'Deleted user logins';

    -- 4. Delete from AspNetUserTokens (user tokens)
    DELETE FROM AspNetUserTokens WHERE UserId = @UserId;
    PRINT 'Deleted user tokens';

    -- 5. Delete from SocialMedia (CASCADE configured, but being explicit)
    DELETE FROM dbo.UserSocialMediaSites WHERE UserId = @UserId;
    PRINT 'Deleted social media links';

    -- 6. Delete from UserExpertise (CASCADE configured, but being explicit)
    DELETE FROM UserExpertises WHERE UserId = @UserId;
    PRINT 'Deleted user expertise relationships';

    -- 7. Handle Mentorships - These have RESTRICT delete behavior
    -- Update mentorships to set user references to NULL or handle appropriately
    -- Option 1: Set status to 'Cancelled' and keep for historical records
    UPDATE Mentorships 
    SET Status = 'Cancelled', 
        Notes = COALESCE(Notes + '; ', '') + 'User account deleted on ' + CAST(GETUTCDATE() AS VARCHAR(30))
    WHERE MentorId = @UserId OR NewSpeakerId = @UserId;
    
    -- Option 2: If you prefer to delete mentorship records entirely, uncomment below:
    -- DELETE FROM Mentorships WHERE MentorId = @UserId OR NewSpeakerId = @UserId;
    
    PRINT 'Updated mentorships (set to cancelled)';

    -- 8. Finally, delete the user from AspNetUsers
    DELETE FROM AspNetUsers WHERE Id = @UserId;
    PRINT 'Deleted user record';

    -- Commit the transaction
    COMMIT TRANSACTION;
    PRINT 'User deletion completed successfully!';
    
END TRY
BEGIN CATCH
    -- Rollback on error
    ROLLBACK TRANSACTION;
    
    PRINT 'Error occurred during user deletion. Transaction rolled back.';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    
END CATCH;

-- Verification query - should return no results if deletion was successful
SELECT COUNT(*) as RemainingUserRecords FROM AspNetUsers WHERE Id = @UserId;