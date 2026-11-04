using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class MeetingDecision : BaseEntity
{
    private MeetingDecision()
    {
    }

    public MeetingDecision(
        int meetingSessionId,
        string title,
        string? description,
        string decidedByUserId,
        int? agendaItemId = null,
        int? projectId = null)
    {
        if (meetingSessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(meetingSessionId));

        if (string.IsNullOrWhiteSpace(decidedByUserId))
            throw new ArgumentException(
                "Decision owner is required.",
                nameof(decidedByUserId));

        MeetingSessionId = meetingSessionId;
        SetTitle(title);

        Description = description?.Trim();
        DecidedByUserId = decidedByUserId.Trim();

        AgendaItemId = agendaItemId;
        ProjectId = projectId;

        Status = MeetingDecisionStatus.Active;
        DecidedAt = DateTime.UtcNow;
    }

    public int MeetingSessionId { get; private set; }

    public MeetingSession MeetingSession { get; private set; } = null!;

    public int? AgendaItemId { get; private set; }

    public int? ProjectId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public string DecidedByUserId { get; private set; } = null!;

    public DateTime DecidedAt { get; private set; }

    public MeetingDecisionStatus Status { get; private set; }

    public int? SupersededByDecisionId { get; private set; }

    public void Supersede(int newDecisionId)
    {
        if (newDecisionId <= 0 || newDecisionId == Id)
            throw new ArgumentOutOfRangeException(nameof(newDecisionId));

        if (Status != MeetingDecisionStatus.Active)
            throw new InvalidOperationException(
                "Only active decisions can be superseded.");

        SupersededByDecisionId = newDecisionId;
        Status = MeetingDecisionStatus.Superseded;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = MeetingDecisionStatus.Archived;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Decision title is required.",
                nameof(title));

        Title = title.Trim();
    }
}