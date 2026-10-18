IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    CREATE TABLE [Boards] (
        [Id] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_Boards] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    CREATE TABLE [Projects] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_Projects] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    CREATE TABLE [BoardColumns] (
        [Id] int NOT NULL IDENTITY,
        [BoardId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Order] int NOT NULL,
        [WipLimit] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_BoardColumns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BoardColumns_Boards_BoardId] FOREIGN KEY ([BoardId]) REFERENCES [Boards] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkItems] (
        [Id] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [BoardColumnId] int NULL,
        [Title] nvarchar(300) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Type] int NOT NULL,
        [Priority] int NOT NULL,
        [DueDate] datetime2 NULL,
        [IsCompleted] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_WorkItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkItems_BoardColumns_BoardColumnId] FOREIGN KEY ([BoardColumnId]) REFERENCES [BoardColumns] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_WorkItems_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BoardColumns_BoardId] ON [BoardColumns] ([BoardId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkItems_BoardColumnId] ON [WorkItems] ([BoardColumnId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkItems_ProjectId] ON [WorkItems] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920084911_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920084911_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920103613_AddIdentity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920103613_AddIdentity', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920105145_AddWorkItemAssignment'
)
BEGIN
    ALTER TABLE [WorkItems] ADD [AssignedToUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920105145_AddWorkItemAssignment'
)
BEGIN
    CREATE INDEX [IX_WorkItems_AssignedToUserId] ON [WorkItems] ([AssignedToUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920105145_AddWorkItemAssignment'
)
BEGIN
    ALTER TABLE [WorkItems] ADD CONSTRAINT [FK_WorkItems_AspNetUsers_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920105145_AddWorkItemAssignment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920105145_AddWorkItemAssignment', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920115944_AddExperiments'
)
BEGIN
    CREATE TABLE [Experiments] (
        [Id] int NOT NULL IDENTITY,
        [WorkItemId] int NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Protocol] nvarchar(max) NULL,
        [StartedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_Experiments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Experiments_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [WorkItems] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920115944_AddExperiments'
)
BEGIN
    CREATE INDEX [IX_Experiments_WorkItemId] ON [Experiments] ([WorkItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920115944_AddExperiments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920115944_AddExperiments', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920122620_AddSamples'
)
BEGIN
    CREATE TABLE [Samples] (
        [Id] int NOT NULL IDENTITY,
        [ExperimentId] int NOT NULL,
        [SampleCode] nvarchar(100) NOT NULL,
        [Name] nvarchar(300) NULL,
        [SampleType] nvarchar(200) NULL,
        [Matrix] nvarchar(300) NULL,
        [PreparationMethod] nvarchar(max) NULL,
        [CollectedAt] datetime2 NULL,
        [ExternalReference] nvarchar(300) NULL,
        [Description] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_Samples] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Samples_Experiments_ExperimentId] FOREIGN KEY ([ExperimentId]) REFERENCES [Experiments] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920122620_AddSamples'
)
BEGIN
    CREATE INDEX [IX_Samples_ExperimentId] ON [Samples] ([ExperimentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920122620_AddSamples'
)
BEGIN
    CREATE INDEX [IX_Samples_SampleCode] ON [Samples] ([SampleCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920122620_AddSamples'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920122620_AddSamples', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920124008_AddResults'
)
BEGIN
    CREATE TABLE [Results] (
        [Id] int NOT NULL IDENTITY,
        [SampleId] int NOT NULL,
        [MetricName] nvarchar(300) NOT NULL,
        [NumericValue] decimal(18,6) NULL,
        [TextValue] nvarchar(max) NULL,
        [Unit] nvarchar(100) NULL,
        [Method] nvarchar(1000) NULL,
        [Status] nvarchar(100) NOT NULL,
        [Evidence] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_Results] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Results_Samples_SampleId] FOREIGN KEY ([SampleId]) REFERENCES [Samples] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920124008_AddResults'
)
BEGIN
    CREATE INDEX [IX_Results_MetricName] ON [Results] ([MetricName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920124008_AddResults'
)
BEGIN
    CREATE INDEX [IX_Results_SampleId] ON [Results] ([SampleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920124008_AddResults'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920124008_AddResults', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920130009_AddAttachments'
)
BEGIN
    CREATE TABLE [Attachments] (
        [Id] int NOT NULL IDENTITY,
        [WorkItemId] int NOT NULL,
        [OriginalFileName] nvarchar(500) NOT NULL,
        [StoredFileName] nvarchar(200) NOT NULL,
        [ContentType] nvarchar(200) NOT NULL,
        [FileSize] bigint NOT NULL,
        [StoragePath] nvarchar(1000) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [UploadedByUserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_Attachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Attachments_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [WorkItems] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920130009_AddAttachments'
)
BEGIN
    CREATE INDEX [IX_Attachments_StoredFileName] ON [Attachments] ([StoredFileName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920130009_AddAttachments'
)
BEGIN
    CREATE INDEX [IX_Attachments_WorkItemId] ON [Attachments] ([WorkItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920130009_AddAttachments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920130009_AddAttachments', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920132733_AddDecisionLogs'
)
BEGIN
    CREATE TABLE [DecisionLogs] (
        [Id] int NOT NULL IDENTITY,
        [WorkItemId] int NOT NULL,
        [DecisionType] nvarchar(200) NOT NULL,
        [Decision] nvarchar(max) NOT NULL,
        [Rationale] nvarchar(max) NULL,
        [Evidence] nvarchar(max) NULL,
        [CreatedByUserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_DecisionLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DecisionLogs_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [WorkItems] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920132733_AddDecisionLogs'
)
BEGIN
    CREATE INDEX [IX_DecisionLogs_CreatedAt] ON [DecisionLogs] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920132733_AddDecisionLogs'
)
BEGIN
    CREATE INDEX [IX_DecisionLogs_WorkItemId] ON [DecisionLogs] ([WorkItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920132733_AddDecisionLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920132733_AddDecisionLogs', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928141838_AddWorkItemTransitions'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928141838_AddWorkItemTransitions'
)
BEGIN
    CREATE INDEX [IX_WorkItemTransitions_ChangedAtUtc] ON [WorkItemTransitions] ([ChangedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928141838_AddWorkItemTransitions'
)
BEGIN
    CREATE INDEX [IX_WorkItemTransitions_ChangedByUserId] ON [WorkItemTransitions] ([ChangedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928141838_AddWorkItemTransitions'
)
BEGIN
    CREATE INDEX [IX_WorkItemTransitions_FromBoardColumnId] ON [WorkItemTransitions] ([FromBoardColumnId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928141838_AddWorkItemTransitions'
)
BEGIN
    CREATE INDEX [IX_WorkItemTransitions_ToBoardColumnId] ON [WorkItemTransitions] ([ToBoardColumnId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928141838_AddWorkItemTransitions'
)
BEGIN
    CREATE INDEX [IX_WorkItemTransitions_WorkItemId] ON [WorkItemTransitions] ([WorkItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260928141838_AddWorkItemTransitions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260928141838_AddWorkItemTransitions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    IF SCHEMA_ID(N'pubbo') IS NULL EXEC(N'CREATE SCHEMA [pubbo];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    ALTER TABLE [Projects] ADD [EndDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    ALTER TABLE [Projects] ADD [StartDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Action] ON [pubbo].[AuditLogs] ([Action]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [pubbo].[AuditLogs] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [pubbo].[AuditLogs] ([EntityType], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [pubbo].[AuditLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_ResearchReviews_RequestedAtUtc] ON [pubbo].[ResearchReviews] ([RequestedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_ResearchReviews_Status] ON [pubbo].[ResearchReviews] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_ResearchReviews_WorkItemId] ON [pubbo].[ResearchReviews] ([WorkItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_ResearchReviews_WorkItemId_Status] ON [pubbo].[ResearchReviews] ([WorkItemId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_WorkItemBlocks_BlockedAtUtc] ON [WorkItemBlocks] ([BlockedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_WorkItemBlocks_BlockedByUserId] ON [WorkItemBlocks] ([BlockedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_WorkItemBlocks_UnblockedByUserId] ON [WorkItemBlocks] ([UnblockedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_WorkItemBlocks_WorkItemId] ON [WorkItemBlocks] ([WorkItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    CREATE INDEX [IX_WorkItemBlocks_WorkItemId_UnblockedAtUtc] ON [WorkItemBlocks] ([WorkItemId], [UnblockedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004154853_AddProjectSchedule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004154853_AddProjectSchedule', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018161914_SetDefaultBoardColumnWipLimits'
)
BEGIN
    UPDATE BoardColumns
    SET WipLimit = 5
    WHERE Name = 'Ready'
      AND WipLimit IS NULL;

    UPDATE BoardColumns
    SET WipLimit = 4
    WHERE Name = 'In Progress'
      AND WipLimit IS NULL;

    UPDATE BoardColumns
    SET WipLimit = 3
    WHERE Name = 'Review / Validation'
      AND WipLimit IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018161914_SetDefaultBoardColumnWipLimits'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261018161914_SetDefaultBoardColumnWipLimits', N'10.0.12');
END;

COMMIT;
GO

