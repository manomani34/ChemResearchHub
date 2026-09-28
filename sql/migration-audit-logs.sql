/*
    ChemResearchHub - Phase 6
    Audit Logs
    Target schema: pubbo
*/

IF OBJECT_ID(N'[pubbo].[AuditLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [pubbo].[AuditLogs]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [EntityType] NVARCHAR(100) NOT NULL,
        [EntityId] INT NULL,
        [Description] NVARCHAR(2000) NULL,
        [Metadata] NVARCHAR(8000) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [ModifiedAt] DATETIME2 NULL,

        CONSTRAINT [PK_AuditLogs]
            PRIMARY KEY ([Id]),

        CONSTRAINT [FK_AuditLogs_AspNetUsers_UserId]
            FOREIGN KEY ([UserId])
            REFERENCES [pubbo].[AspNetUsers] ([Id])
            ON DELETE SET NULL
    );

    CREATE INDEX [IX_AuditLogs_CreatedAt]
        ON [pubbo].[AuditLogs] ([CreatedAt]);

    CREATE INDEX [IX_AuditLogs_Action]
        ON [pubbo].[AuditLogs] ([Action]);

    CREATE INDEX [IX_AuditLogs_Entity]
        ON [pubbo].[AuditLogs] ([EntityType], [EntityId]);
END
GO
