BEGIN TRANSACTION;
IF SCHEMA_ID(N'pubbo') IS NULL EXEC(N'CREATE SCHEMA [pubbo];');

ALTER TABLE [Projects] ADD [EndDate] datetime2 NULL;

ALTER TABLE [Projects] ADD [StartDate] datetime2 NULL;

CREATE TABLE [pubbo].[AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] int NULL,
    [Description] nvarchar(2000) NULL,
    [Metadata] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [pubbo].[ResearchReviews] (
    [Id] int NOT NULL IDENTITY,
    [WorkItemId] int NOT NULL,
    [RequestedByUserId] nvarchar(450) NOT NULL,
    [ReviewerUserId] nvarchar(450) NULL,
    [Status] nvarchar(32) NOT NULL,
    [RequestedAtUtc] datetime2 NOT NULL,
    [StartedAtUtc] datetime2 NULL,
    [ReviewedAtUtc] datetime2 NULL,
    [ReviewNote] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_ResearchReviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ResearchReviews_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [WorkItems] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [WorkItemBlocks] (
    [Id] int NOT NULL IDENTITY,
    [WorkItemId] int NOT NULL,
    [Reason] nvarchar(1000) NOT NULL,
    [BlockedByUserId] nvarchar(450) NULL,
    [BlockedAtUtc] datetime2 NOT NULL,
    [UnblockedByUserId] nvarchar(450) NULL,
    [UnblockedAtUtc] datetime2 NULL,
    [UnblockNote] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_WorkItemBlocks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkItemBlocks_AspNetUsers_BlockedByUserId] FOREIGN KEY ([BlockedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_WorkItemBlocks_AspNetUsers_UnblockedByUserId] FOREIGN KEY ([UnblockedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_WorkItemBlocks_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [WorkItems] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AuditLogs_Action] ON [pubbo].[AuditLogs] ([Action]);

CREATE INDEX [IX_AuditLogs_CreatedAt] ON [pubbo].[AuditLogs] ([CreatedAt]);

CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [pubbo].[AuditLogs] ([EntityType], [EntityId]);

CREATE INDEX [IX_AuditLogs_UserId] ON [pubbo].[AuditLogs] ([UserId]);

CREATE INDEX [IX_ResearchReviews_RequestedAtUtc] ON [pubbo].[ResearchReviews] ([RequestedAtUtc]);

CREATE INDEX [IX_ResearchReviews_Status] ON [pubbo].[ResearchReviews] ([Status]);

CREATE INDEX [IX_ResearchReviews_WorkItemId] ON [pubbo].[ResearchReviews] ([WorkItemId]);

CREATE INDEX [IX_ResearchReviews_WorkItemId_Status] ON [pubbo].[ResearchReviews] ([WorkItemId], [Status]);

CREATE INDEX [IX_WorkItemBlocks_BlockedAtUtc] ON [WorkItemBlocks] ([BlockedAtUtc]);

CREATE INDEX [IX_WorkItemBlocks_BlockedByUserId] ON [WorkItemBlocks] ([BlockedByUserId]);

CREATE INDEX [IX_WorkItemBlocks_UnblockedByUserId] ON [WorkItemBlocks] ([UnblockedByUserId]);

CREATE INDEX [IX_WorkItemBlocks_WorkItemId] ON [WorkItemBlocks] ([WorkItemId]);

CREATE INDEX [IX_WorkItemBlocks_WorkItemId_UnblockedAtUtc] ON [WorkItemBlocks] ([WorkItemId], [UnblockedAtUtc]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261004154853_AddProjectSchedule', N'10.0.12');

COMMIT;
GO

