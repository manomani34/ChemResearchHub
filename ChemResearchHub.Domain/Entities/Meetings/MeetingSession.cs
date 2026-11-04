using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class MeetingSession : BaseEntity
{
    private MeetingSession()
    {
    }

    public MeetingSession(
        string title,
        DateTime scheduledStartAt,
        int plannedDurationMinutes,
        int? meetingSeriesId = null,
        string? description = null)
    {
        SetTitle(title);

        if (plannedDurationMinutes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(plannedDurationMinutes));

        ScheduledStartAt = EnsureUtc(scheduledStartAt);
        PlannedDurationMinutes = plannedDurationMinutes;
        MeetingSeriesId = meetingSeriesId;
        Description = description?.Trim();

        Status = MeetingSessionStatus.Draft;
        TimerState = MeetingTimerState.NotStarted;
        CurrentAgendaItemIndex = -1;
    }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public int? MeetingSeriesId { get; private set; }

    public MeetingSeries? MeetingSeries { get; private set; }

    public DateTime ScheduledStartAt { get; private set; }

    public int PlannedDurationMinutes { get; private set; }

    public DateTime? ActualStartedAt { get; private set; }

    public DateTime? ActualEndedAt { get; private set; }

    public MeetingSessionStatus Status { get; private set; }

    public MeetingTimerState TimerState { get; private set; }

    public DateTime? TimerPausedAt { get; private set; }

    public int AccumulatedPauseSeconds { get; private set; }

    public int CurrentAgendaItemIndex { get; private set; }

    public string? FacilitatorUserId { get; private set; }

    public string? CreatedByUserId { get; private set; }

    public ICollection<AgendaItem> AgendaItems { get; private set; }
        = new List<AgendaItem>();

    public ICollection<MeetingParticipant> Participants { get; private set; }
        = new List<MeetingParticipant>();

    public ICollection<ParkingLotItem> ParkingLotItems { get; private set; }
        = new List<ParkingLotItem>();

    public ICollection<MeetingDecision> Decisions { get; private set; }
        = new List<MeetingDecision>();

    public ICollection<MeetingActionItem> ActionItems { get; private set; }
        = new List<MeetingActionItem>();

    public ICollection<MeetingTimerEvent> TimerEvents { get; private set; }
        = new List<MeetingTimerEvent>();

    public void Update(
        string title,
        string? description,
        DateTime scheduledStartAt,
        int plannedDurationMinutes)
    {
        EnsureEditable();

        SetTitle(title);

        if (plannedDurationMinutes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(plannedDurationMinutes));

        Description = description?.Trim();
        ScheduledStartAt = EnsureUtc(scheduledStartAt);
        PlannedDurationMinutes = plannedDurationMinutes;

        ModifiedAt = DateTime.UtcNow;
    }

    public void SetFacilitator(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "Facilitator user id is required.",
                nameof(userId));

        FacilitatorUserId = userId.Trim();
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetCreatedBy(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "Creator user id is required.",
                nameof(userId));

        CreatedByUserId = userId.Trim();
        ModifiedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status != MeetingSessionStatus.Draft)
            throw new InvalidOperationException(
                "Only draft meetings can be published.");

        if (!AgendaItems.Any())
            throw new InvalidOperationException(
                "At least one agenda item is required.");

        Status = MeetingSessionStatus.Published;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Start(DateTime? now = null)
    {
        if (Status != MeetingSessionStatus.Published)
            throw new InvalidOperationException(
                "Only published meetings can be started.");

        var current = EnsureUtc(now ?? DateTime.UtcNow);

        Status = MeetingSessionStatus.InProgress;
        TimerState = MeetingTimerState.Running;
        ActualStartedAt = current;
        TimerPausedAt = null;
        AccumulatedPauseSeconds = 0;
        ModifiedAt = current;
    }

    public void Pause(DateTime? now = null)
    {
        if (Status != MeetingSessionStatus.InProgress ||
            TimerState != MeetingTimerState.Running)
        {
            throw new InvalidOperationException(
                "Meeting timer is not running.");
        }

        TimerPausedAt = EnsureUtc(now ?? DateTime.UtcNow);
        TimerState = MeetingTimerState.Paused;
        ModifiedAt = TimerPausedAt.Value;
    }

    public void Resume(DateTime? now = null)
    {
        if (Status != MeetingSessionStatus.InProgress ||
            TimerState != MeetingTimerState.Paused ||
            !TimerPausedAt.HasValue)
        {
            throw new InvalidOperationException(
                "Meeting timer is not paused.");
        }

        var current = EnsureUtc(now ?? DateTime.UtcNow);

        var pausedSeconds = (int)Math.Max(
            0,
            (current - TimerPausedAt.Value).TotalSeconds);

        AccumulatedPauseSeconds += pausedSeconds;

        TimerPausedAt = null;
        TimerState = MeetingTimerState.Running;
        ModifiedAt = current;
    }

    public void SetCurrentAgendaItemIndex(int index)
    {
        if (index < -1)
            throw new ArgumentOutOfRangeException(nameof(index));

        CurrentAgendaItemIndex = index;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Complete(DateTime? now = null)
    {
        if (Status != MeetingSessionStatus.InProgress)
            throw new InvalidOperationException(
                "Only an in-progress meeting can be completed.");

        var current = EnsureUtc(now ?? DateTime.UtcNow);

        if (TimerState == MeetingTimerState.Paused &&
            TimerPausedAt.HasValue)
        {
            AccumulatedPauseSeconds += (int)Math.Max(
                0,
                (current - TimerPausedAt.Value).TotalSeconds);
        }

        ActualEndedAt = current;
        TimerPausedAt = null;
        TimerState = MeetingTimerState.Completed;
        Status = MeetingSessionStatus.Completed;
        ModifiedAt = current;
    }

    public void Cancel()
    {
        if (Status == MeetingSessionStatus.InProgress ||
            Status == MeetingSessionStatus.Completed)
        {
            throw new InvalidOperationException(
                "This meeting cannot be cancelled.");
        }

        Status = MeetingSessionStatus.Cancelled;
        ModifiedAt = DateTime.UtcNow;
    }

    public int GetElapsedSeconds(DateTime? now = null)
    {
        if (!ActualStartedAt.HasValue)
            return 0;

        var end = TimerState == MeetingTimerState.Paused &&
                  TimerPausedAt.HasValue
            ? TimerPausedAt.Value
            : ActualEndedAt ?? EnsureUtc(now ?? DateTime.UtcNow);

        var elapsed = (int)Math.Max(
            0,
            (end - ActualStartedAt.Value).TotalSeconds);

        return Math.Max(0, elapsed - AccumulatedPauseSeconds);
    }

    public int GetRemainingSeconds(DateTime? now = null)
    {
        var total = PlannedDurationMinutes * 60;
        return Math.Max(0, total - GetElapsedSeconds(now));
    }

    private void EnsureEditable()
    {
        if (Status == MeetingSessionStatus.InProgress ||
            Status == MeetingSessionStatus.Completed ||
            Status == MeetingSessionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "This meeting can no longer be edited.");
        }
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Meeting title is required.",
                nameof(title));

        Title = title.Trim();
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}