using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums;

namespace ChemResearchHub.Domain.Entities.WorkItem;

public class WorkItem : BaseEntity
{
    private WorkItem()
    {
    }

    public WorkItem(
        int projectId,
        string title,
        WorkItemType type = WorkItemType.Task)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException(
                "ProjectId must be greater than zero.",
                nameof(projectId));
        }

        SetTitle(title);

        ProjectId = projectId;
        Type = type;
    }

    public int ProjectId { get; private set; }

    public int? BoardColumnId { get; private set; }

    public string? AssignedToUserId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public WorkItemType Type { get; private set; }

    public int Priority { get; private set; }

    public DateTime? DueDate { get; private set; }

    public bool IsCompleted { get; private set; }

    public int SortOrder { get; private set; }

    public void Update(
        string title,
        string? description,
        WorkItemType type,
        int priority,
        DateTime? dueDate)
    {
        SetTitle(title);

        Description = description;
        Type = type;
        Priority = priority;
        DueDate = dueDate;

        ModifiedAt = DateTime.UtcNow;
    }

    public void AssignTo(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "UserId is required.",
                nameof(userId));
        }

        AssignedToUserId = userId.Trim();

        ModifiedAt = DateTime.UtcNow;
    }

    public void Unassign()
    {
        AssignedToUserId = null;

        ModifiedAt = DateTime.UtcNow;
    }

    public void MoveToColumn(
        int boardColumnId,
        int sortOrder)
    {
        if (boardColumnId <= 0)
        {
            throw new ArgumentException(
                "BoardColumnId must be greater than zero.",
                nameof(boardColumnId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Sort order cannot be negative.");
        }

        BoardColumnId = boardColumnId;
        SortOrder = sortOrder;

        ModifiedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        IsCompleted = true;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsCompleted = false;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Work item title is required.",
                nameof(title));
        }

        Title = title.Trim();
    }
}