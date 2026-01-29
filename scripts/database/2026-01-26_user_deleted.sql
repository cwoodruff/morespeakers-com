-- Add Soft-Delete support
ALTER TABLE [AspNetUsers] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
ALTER TABLE [AspNetUsers] ADD [DeletedAt] DATETIMEOFFSET NULL;

-- Add index for Soft-Delete performance
CREATE INDEX [IX_AspNetUsers_IsDeleted] ON [AspNetUsers] ([IsDeleted]);

-- Add MustChangePassword support (from previous security updates)
ALTER TABLE [AspNetUsers] ADD [MustChangePassword] BIT NOT NULL DEFAULT 0;
