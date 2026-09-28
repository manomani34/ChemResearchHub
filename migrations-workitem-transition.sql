BEGIN TRANSACTION;
CREATE TABLE [WorkItemTransitions] (
    [Id] int NOT NULL IDENTITY,
    [WorkItemId] int NOT NULL,
    [FromBoardColumnId] int NULL,
    [ToBoardColumnId] int NOT NULL,
    [ChangedByUserId] nvarchar(450) NULL,
    [ChangedAtUtc] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_WorkItemTransitions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkItemTransitions_AspNetUsers_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_WorkItemTransitions_BoardColumns_FromBoardColumnId] FOREIGN KEY ([FromBoardColumnId]) REFERENCES [BoardColumns] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkItemTransitions_BoardColumns_ToBoardColumnId] FOREIGN KEY ([ToBoardColumnId]) REFERENCES [BoardColumns] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WorkItemTransitions_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [WorkItems] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_WorkItemTransitions_ChangedAtUtc] ON [WorkItemTransitions] ([ChangedAtUtc]);

CREATE INDEX [IX_WorkItemTransitions_ChangedByUserId] ON [WorkItemTransitions] ([ChangedByUserId]);

CREATE INDEX [IX_WorkItemTransitions_FromBoardColumnId] ON [WorkItemTransitions] ([FromBoardColumnId]);

CREATE INDEX [IX_WorkItemTransitions_ToBoardColumnId] ON [WorkItemTransitions] ([ToBoardColumnId]);

CREATE INDEX [IX_WorkItemTransitions_WorkItemId] ON [WorkItemTransitions] ([WorkItemId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260928141838_AddWorkItemTransitions', N'10.0.12');

COMMIT;
GO

