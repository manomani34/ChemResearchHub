using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.WorkItemAssignment;

public class WorkItemAssignment : BaseEntity
{
    private WorkItemAssignment()
    {
    }

    public WorkItemAssignment(
        int workItemId,
        string userId,
        string? assignedByUserId = null)
    {
        if (workItemId <= 0)
        {
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "UserId is required.",
                nameof(userId));
        }

        WorkItemId = workItemId;
        UserId = userId.Trim();
        AssignedByUserId = string.IsNullOrWhiteSpace(assignedByUserId)
            ? null
            : assignedByUserId.Trim();

        AssignedAt = DateTime.UtcNow;
    }

    public int WorkItemId { get; private set; }

    public string UserId { get; private set; } = null!;

    public string? AssignedByUserId { get; private set; }

    public DateTime AssignedAt { get; private set; }
}
