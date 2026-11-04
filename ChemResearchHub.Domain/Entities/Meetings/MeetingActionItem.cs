
using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class MeetingActionItem : BaseEntity
{
    private MeetingActionItem()
    {
    }

    public MeetingActionItem(
        int meetingSessionId,
        string title,
        string? description,
        string? assignedToUserId,
        DateTime? dueDate = null,
        int? projectId = null,
        int? decisionId = null,
        string? acceptanceCriteria = null)
    {
        if (meetingSessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(meetingSessionId));

        MeetingSessionId = meetingSessionId;
        SetTitle(title);

        Description = description?.Trim();
        AssignedToUserId = Normalize(assignedToUserId);
        DueDate = dueDate;
        ProjectId = projectId;
        MeetingDecisionId = decisionId;
        AcceptanceCriteria = acceptanceCriteria?.Trim();

        Status = MeetingActionItemStatus.Open;
        Type = MeetingActionItemType.Independent;
    }

    public int MeetingSessionId { get; private set; }

    public MeetingSession MeetingSession { get; private set; } = null!;

    public int? MeetingDecisionId { get; private set; }

    public int? ProjectId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? AssignedToUserId { get; private set; }

    public DateTime? DueDate { get; private set; }

    public string? AcceptanceCriteria { get; private set; }

    public MeetingActionItemStatus Status { get; private set; }

    public MeetingActionItemType Type { get; private set; }

    public int? WorkItemId { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public void Update(
        string title,
        string? description,
        string? assignedToUserId,
        DateTime? dueDate,
        int? projectId,
        int? decisionId,
        string? acceptanceCriteria)
    {
        SetTitle(title);

        Description = description?.Trim();
        AssignedToUserId = Normalize(assignedToUserId);
        DueDate = dueDate;
        ProjectId = projectId;
        MeetingDecisionId = decisionId;
        AcceptanceCriteria = acceptanceCriteria?.Trim();

        ModifiedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status == MeetingActionItemStatus.Completed ||
            Status == MeetingActionItemStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "This action item cannot be started.");
        }

        Status = MeetingActionItemStatus.InProgress;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        if (Status == MeetingActionItemStatus.Completed ||
            Status == MeetingActionItemStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "This action item cannot be blocked.");
        }

        Status = MeetingActionItemStatus.Blocked;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = MeetingActionItemStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = MeetingActionItemStatus.Cancelled;
        ModifiedAt = DateTime.UtcNow;
    }

    public void LinkWorkItem(int workItemId)
    {
        if (workItemId <= 0)
            throw new ArgumentOutOfRangeException(nameof(workItemId));

        if (Type == MeetingActionItemType.ConvertedWorkItem)
        {
            throw new InvalidOperationException(
                "A converted action item cannot change its linked work item.");
        }

        WorkItemId = workItemId;
        Type = MeetingActionItemType.LinkedWorkItem;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UnlinkWorkItem()
    {
        if (!WorkItemId.HasValue)
            return;

        if (Type == MeetingActionItemType.ConvertedWorkItem)
        {
            throw new InvalidOperationException(
                "A converted action item cannot be unlinked.");
        }

        WorkItemId = null;
        Type = MeetingActionItemType.Independent;
        ModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsConvertedWorkItem(int workItemId)
    {
        if (workItemId <= 0)
            throw new ArgumentOutOfRangeException(nameof(workItemId));

        if (WorkItemId.HasValue)
            throw new InvalidOperationException(
                "This action item is already linked to a work item.");

        WorkItemId = workItemId;
        Type = MeetingActionItemType.ConvertedWorkItem;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Action item title is required.",
                nameof(title));

        Title = title.Trim();
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
