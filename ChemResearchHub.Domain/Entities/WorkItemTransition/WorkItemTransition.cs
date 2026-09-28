using ChemResearchHub.Domain.Common;
using WorkItemEntity = ChemResearchHub.Domain.Entities.WorkItem.WorkItem;

namespace ChemResearchHub.Domain.Entities.WorkItemTransition;

public class WorkItemTransition : BaseEntity
{
    private WorkItemTransition()
    {
    }

    public WorkItemTransition(
        WorkItemEntity workItem,
        int? fromBoardColumnId,
        int toBoardColumnId,
        string? changedByUserId)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        if (fromBoardColumnId.HasValue && fromBoardColumnId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fromBoardColumnId),
                "From board column id must be greater than zero when provided.");
        }

        if (toBoardColumnId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toBoardColumnId),
                "To board column id must be greater than zero.");
        }

        WorkItem = workItem;
        WorkItemId = workItem.Id;
        FromBoardColumnId = fromBoardColumnId;
        ToBoardColumnId = toBoardColumnId;

        ChangedByUserId = string.IsNullOrWhiteSpace(changedByUserId)
            ? null
            : changedByUserId.Trim();

        ChangedAtUtc = DateTime.UtcNow;
    }

    public int WorkItemId { get; private set; }

    public WorkItemEntity WorkItem { get; private set; } = null!;

    public int? FromBoardColumnId { get; private set; }

    public int ToBoardColumnId { get; private set; }

    public string? ChangedByUserId { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }
}