using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.WorkItemUserAlert;

public class WorkItemUserAlert : BaseEntity
{
    private WorkItemUserAlert()
    {
    }

    public WorkItemUserAlert(int workItemId, string userId)
    {
        if (workItemId <= 0)
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "UserId is required.",
                nameof(userId));

        WorkItemId = workItemId;
        UserId = userId.Trim();
    }

    public int WorkItemId { get; private set; }

    public string UserId { get; private set; } = null!;

    public DateTime? ReadAt { get; private set; }

    public bool IsRead => ReadAt.HasValue;

    public void MarkAsRead()
    {
        if (!ReadAt.HasValue)
            ReadAt = DateTime.UtcNow;
    }

    public void MarkAsUnread()
    {
        ReadAt = null;
    }
}