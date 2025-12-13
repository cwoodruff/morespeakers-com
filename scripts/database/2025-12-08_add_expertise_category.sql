-- Create ExpertiseCategories table
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'ExpertiseCategories' AND type = 'U')
BEGIN
    CREATE TABLE dbo.ExpertiseCategories (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500) NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_ExpertiseCategories_CreatedDate DEFAULT (SYSUTCDATETIME()),
        IsActive BIT NOT NULL CONSTRAINT DF_ExpertiseCategories_IsActive DEFAULT (1)
    );
END
GO

-- Ensure Expertises table exists
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'Expertises' AND type = 'U')
BEGIN
    RAISERROR('Table Expertises does not exist. Run base schema first.', 16, 1);
END
GO

-- Add ExpertiseCategoryId column to Expertises if missing
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.objects t ON c.object_id = t.object_id
    WHERE t.name = 'Expertises' AND c.name = 'ExpertiseCategoryId')
BEGIN
    ALTER TABLE dbo.Expertises ADD ExpertiseCategoryId INT NULL;
END
GO

-- Create a default category if none exists, and set existing Expertises to that category
DECLARE @DefaultCategoryId INT;
IF NOT EXISTS (SELECT 1 FROM dbo.ExpertiseCategories WHERE Name = 'General')
BEGIN
    SET IDENTITY_INSERT dbo.ExpertiseCategories ON;
    INSERT INTO dbo.ExpertiseCategories (Id, Name, Description) VALUES (1, 'General', 'Default category for existing expertise');
    SET IDENTITY_INSERT dbo.ExpertiseCategories OFF;
END

SELECT TOP 1 @DefaultCategoryId = Id FROM dbo.ExpertiseCategories WHERE Name = 'General';

-- Backfill nulls
UPDATE e
SET e.ExpertiseCategoryId = @DefaultCategoryId
FROM dbo.Expertises e
WHERE e.ExpertiseCategoryId IS NULL;
GO

-- Make column NOT NULL
ALTER TABLE dbo.Expertises ALTER COLUMN ExpertiseCategoryId INT NOT NULL;
GO

-- Add index on ExpertiseCategoryId if missing
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_Expertises_ExpertiseCategoryId' AND object_id = OBJECT_ID('dbo.Expertises'))
BEGIN
    CREATE INDEX IX_Expertises_ExpertiseCategoryId ON dbo.Expertises (ExpertiseCategoryId);
END
GO

-- Add FK constraint if missing
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Expertises_ExpertiseCategories_ExpertiseCategoryId')
BEGIN
    ALTER TABLE dbo.Expertises WITH CHECK ADD CONSTRAINT FK_Expertises_ExpertiseCategories_ExpertiseCategoryId
        FOREIGN KEY (ExpertiseCategoryId) REFERENCES dbo.ExpertiseCategories(Id) ON DELETE NO ACTION;
END
GO
