SET NOCOUNT ON;

DECLARE @AdminRoleId UNIQUEIDENTIFIER;
SELECT @AdminRoleId = Id FROM AspNetRoles WHERE NormalizedName = 'ADMINISTRATOR';
IF @AdminRoleId IS NULL
BEGIN
    SET @AdminRoleId = NEWID();
    INSERT INTO AspNetRoles (Id, [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (@AdminRoleId, N'Administrator', N'ADMINISTRATOR', CONVERT(nvarchar(128), NEWID()));
END

DECLARE @Admins TABLE (Email NVARCHAR(256));
INSERT INTO @Admins (Email) VALUES
    (N'lead.dev3@yourcompany.com');  -- add/remove as needed

;WITH Targets AS (
    SELECT u.Id AS UserId, a.Email
    FROM AspNetUsers u
    INNER JOIN @Admins a
        ON u.NormalizedEmail = UPPER(a.Email)
)
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT t.UserId, @AdminRoleId
FROM Targets t
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles ur
    WHERE ur.UserId = t.UserId AND ur.RoleId = @AdminRoleId
);
 
-- 4) Report results
SELECT u.Id AS UserId, u.Email, r.Name AS AssignedRole
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.NormalizedName = 'ADMINISTRATOR'
  AND u.NormalizedEmail IN (SELECT UPPER(Email) FROM @Admins)
ORDER BY u.Email;
 
-- Optional: show any emails that did not match a user
SELECT a.Email AS NotFoundEmail
FROM @Admins a
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUsers u WHERE u.NormalizedEmail = UPPER(a.Email)
);