/* =========================================================
   ChemResearchHub - Phase 5
   Research Review & Approval
   Host schema: pubbo
   ========================================================= */

IF OBJECT_ID(N'[pubbo].[ResearchReviews]', N'U') IS NOT NULL
BEGIN
    PRINT 'pubbo.ResearchReviews already exists. No changes made.';
    RETURN;
END;
GO

CREATE TABLE [pubbo].[ResearchReviews]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [WorkItemId] int NOT NULL,
    [RequestedByUserId] nvarchar(450) NOT NULL,
    [ReviewerUserId] nvarchar(450) NULL,
    [Status] nvarchar(32) NOT NULL,
    [RequestedAtUtc] datetime2(7) NOT NULL,
    [StartedAtUtc] datetime2(7) NULL,
    [ReviewedAtUtc] datetime2(7) NULL,
    [ReviewNote] nvarchar(2000) NULL,

    CONSTRAINT [PK_ResearchReviews]
        PRIMARY KEY ([Id]),

    CONSTRAINT [FK_ResearchReviews_WorkItems_WorkItemId]
        FOREIGN KEY ([WorkItemId])
        REFERENCES [pubbo].[WorkItems] ([Id])
        ON DELETE CASCADE,

    CONSTRAINT [FK_ResearchReviews_AspNetUsers_RequestedByUserId]
        FOREIGN KEY ([RequestedByUserId])
        REFERENCES [pubbo].[AspNetUsers] ([Id])
        ON DELETE NO ACTION,

    CONSTRAINT [FK_ResearchReviews_AspNetUsers_ReviewerUserId]
        FOREIGN KEY ([ReviewerUserId])
        REFERENCES [pubbo].[AspNetUsers] ([Id])
        ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_ResearchReviews_WorkItemId]
    ON [pubbo].[ResearchReviews] ([WorkItemId]);
GO

CREATE INDEX [IX_ResearchReviews_RequestedAtUtc]
    ON [pubbo].[ResearchReviews] ([RequestedAtUtc]);
GO

CREATE INDEX [IX_ResearchReviews_Status]
    ON [pubbo].[ResearchReviews] ([Status]);
GO

CREATE INDEX [IX_ResearchReviews_WorkItemId_Status]
    ON [pubbo].[ResearchReviews] ([WorkItemId], [Status]);
GO

/* Only one active review (Pending/InReview) per Work Item. */
CREATE UNIQUE INDEX [UX_ResearchReviews_OneActivePerWorkItem]
    ON [pubbo].[ResearchReviews] ([WorkItemId])
    WHERE [Status] IN (N'Pending', N'InReview');
GO
