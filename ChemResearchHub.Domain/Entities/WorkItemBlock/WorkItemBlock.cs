using ChemResearchHub.Domain.Common;
using WorkItemEntity = ChemResearchHub.Domain.Entities.WorkItem.WorkItem;

namespace ChemResearchHub.Domain.Entities.WorkItemBlock;

public class WorkItemBlock : BaseEntity
{
    private WorkItemBlock() { }

    public WorkItemBlock(WorkItemEntity workItem, string reason, string? blockedByUserId)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Block reason is required.", nameof(reason));
        if (reason.Trim().Length > 1000)
            throw new ArgumentException("Block reason cannot exceed 1000 characters.", nameof(reason));

        WorkItem = workItem;
        WorkItemId = workItem.Id;
        Reason = reason.Trim();
        BlockedByUserId = string.IsNullOrWhiteSpace(blockedByUserId) ? null : blockedByUserId.Trim();
        BlockedAtUtc = DateTime.UtcNow;
    }

    public int WorkItemId { get; private set; }
    public WorkItemEntity WorkItem { get; private set; } = null!;
    public string Reason { get; private set; } = string.Empty;
    public string? BlockedByUserId { get; private set; }
    public DateTime BlockedAtUtc { get; private set; }
    public string? UnblockedByUserId { get; private set; }
    public DateTime? UnblockedAtUtc { get; private set; }
    public string? UnblockNote { get; private set; }

    public void Unblock(string? unblockedByUserId, string? note)
    {
        if (UnblockedAtUtc.HasValue) return;
        if (!string.IsNullOrWhiteSpace(note) && note.Trim().Length > 1000)
            throw new ArgumentException("Unblock note cannot exceed 1000 characters.", nameof(note));

        UnblockedByUserId = string.IsNullOrWhiteSpace(unblockedByUserId) ? null : unblockedByUserId.Trim();
        UnblockNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        UnblockedAtUtc = DateTime.UtcNow;
    }
}
