using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class ParkingLotItem : BaseEntity
{
    private ParkingLotItem()
    {
    }

    public ParkingLotItem(
        int meetingSessionId,
        string title,
        string? summary,
        string raisedByUserId,
        int? agendaItemId = null,
        int? projectId = null)
    {
        if (meetingSessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(meetingSessionId));

        if (string.IsNullOrWhiteSpace(raisedByUserId))
            throw new ArgumentException(
                "Raised by user id is required.",
                nameof(raisedByUserId));

        MeetingSessionId = meetingSessionId;
        SetTitle(title);

        Summary = summary?.Trim();
        RaisedByUserId = raisedByUserId.Trim();

        AgendaItemId = agendaItemId;
        ProjectId = projectId;

        Status = ParkingLotStatus.Submitted;
    }

    public int MeetingSessionId { get; private set; }

    public MeetingSession MeetingSession { get; private set; } = null!;

    public int? AgendaItemId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Summary { get; private set; }

    public string RaisedByUserId { get; private set; } = null!;

    public string? AssignedToUserId { get; private set; }

    public int? ProjectId { get; private set; }

    public int? TargetMeetingSessionId { get; private set; }

    public int Priority { get; private set; } = 2;

    public ParkingLotStatus Status { get; private set; }

    public DateTime? ResolvedAt { get; private set; }


    public void Update(
    string title,
    string? summary,
    int? projectId)
    {
        SetTitle(title);

        Summary = summary?.Trim();
        ProjectId = projectId;

        ModifiedAt = DateTime.UtcNow;
    }

    public void Triage(int priority, string? assignedToUserId = null)
    {
        if (priority < 1 || priority > 3)
            throw new ArgumentOutOfRangeException(nameof(priority));

        Priority = priority;

        AssignedToUserId = string.IsNullOrWhiteSpace(assignedToUserId)
            ? null
            : assignedToUserId.Trim();

        Status = ParkingLotStatus.Triaged;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Schedule(int targetMeetingSessionId)
    {
        if (targetMeetingSessionId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(targetMeetingSessionId));

        TargetMeetingSessionId = targetMeetingSessionId;
        Status = ParkingLotStatus.Scheduled;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Assign(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(nameof(userId));

        AssignedToUserId = userId.Trim();
        Status = ParkingLotStatus.Assigned;
        ModifiedAt = DateTime.UtcNow;
    }

    public void StartReview()
    {
        Status = ParkingLotStatus.UnderReview;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Resolve()
    {
        Status = ParkingLotStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Dismiss()
    {
        Status = ParkingLotStatus.Dismissed;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Parking lot title is required.",
                nameof(title));

        Title = title.Trim();
    }
}