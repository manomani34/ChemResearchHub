BEGIN TRANSACTION;

IF OBJECT_ID(N'[pubbo].[WorkItemBlocks]', N'U') IS NOT NULL
    DROP TABLE [pubbo].[WorkItemBlocks];

DELETE FROM [pubbo].[__EFMigrationsHistory]
WHERE [MigrationId] = N'20260928183000_AddWorkItemBlocks';

CREATE TABLE [pubbo].[WorkItemBlocks] (
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
    CONSTRAINT [FK_WorkItemBlocks_AspNetUsers_BlockedByUserId]
        FOREIGN KEY ([BlockedByUserId]) REFERENCES [pubbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkItemBlocks_AspNetUsers_UnblockedByUserId]
        FOREIGN KEY ([UnblockedByUserId]) REFERENCES [pubbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkItemBlocks_WorkItems_WorkItemId]
        FOREIGN KEY ([WorkItemId]) REFERENCES [pubbo].[WorkItems] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_WorkItemBlocks_WorkItemId]
    ON [pubbo].[WorkItemBlocks] ([WorkItemId]);

CREATE INDEX [IX_WorkItemBlocks_WorkItemId_UnblockedAtUtc]
    ON [pubbo].[WorkItemBlocks] ([WorkItemId], [UnblockedAtUtc]);

CREATE INDEX [IX_WorkItemBlocks_BlockedAtUtc]
    ON [pubbo].[WorkItemBlocks] ([BlockedAtUtc]);

INSERT INTO [pubbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260928183000_AddWorkItemBlocks', N'10.0.12');

COMMIT;
GO
