IF OBJECT_ID('dbo.Sectors', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sectors
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sectors PRIMARY KEY,
        Name VARCHAR(255) NOT NULL,
        Slug VARCHAR(500) NULL,
        Description VARCHAR(255) NULL,
        DisplayOrder INT NOT NULL CONSTRAINT DF_Sectors_DisplayOrder DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_Sectors_IsActive DEFAULT (1)
    );

    CREATE UNIQUE INDEX UX_Sectors_Name ON dbo.Sectors (Name);
    CREATE UNIQUE INDEX UX_Sectors_Slug ON dbo.Sectors (Slug) WHERE Slug IS NOT NULL;
END
GO

-- Add SectorId column to ExpertiseCategories if missing
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
                      JOIN sys.objects t ON c.object_id = t.object_id
    WHERE t.name = 'ExpertiseCategories' AND c.name = 'SectorId')
BEGIN
    ALTER TABLE dbo.ExpertiseCategories ADD SectorId INT NULL
END 
GO

-- Create a default Technology sector
DECLARE @sectorId INT;
IF NOT EXISTS (SELECT 1 FROM dbo.Sectors WHERE Name = 'Technology')
    BEGIN
        SET IDENTITY_INSERT dbo.Sectors ON;
        INSERT INTO dbo.Sectors (Id, Name, Slug, Description, DisplayOrder, IsActive)
        VALUES (1,'Technology', 'technology', 'Technology sector', 1, 1);
    END

SELECT @sectorId = Id FROM dbo.Sectors WHERE Name = 'Technology';

UPDATE dbo.ExpertiseCategories SET SectorId = 1 WHERE SectorId IS NULL;

-- Make column NOT NULL
ALTER TABLE dbo.Expertises ALTER COLUMN ExpertiseCategoryId INT NOT NULL;
    
CREATE INDEX IX_ExpertiseCategories_SectorId ON dbo.ExpertiseCategories (SectorId);

ALTER TABLE dbo.ExpertiseCategories
    ADD CONSTRAINT FK_ExpertiseCategories_Sectors_SectorId
        FOREIGN KEY (SectorId) REFERENCES dbo.Sectors (Id);

GO
