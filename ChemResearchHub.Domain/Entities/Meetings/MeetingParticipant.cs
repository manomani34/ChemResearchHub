using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class MeetingParticipant : BaseEntity
{
    private MeetingParticipant()
    {
    }

    public MeetingParticipant(
        int meetingSessionId,
        string userId,
        MeetingParticipantRole role = MeetingParticipantRole.Participant)
    {
        if (meetingSessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(meetingSessionId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User id is required.",
                nameof(userId));

        MeetingSessionId = meetingSessionId;
        UserId = userId.Trim();
        Role = role;
        IsActive = true;
    }

    public int MeetingSessionId { get; private set; }

    public MeetingSession MeetingSession { get; private set; } = null!;

    public string UserId { get; private set; } = null!;

    public MeetingParticipantRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? JoinedAt { get; private set; }

    public DateTime? LeftAt { get; private set; }

    public void ChangeRole(MeetingParticipantRole role)
    {
        Role = role;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Join(DateTime? now = null)
    {
        if (!IsActive)
            throw new InvalidOperationException(
                "Inactive participant cannot join.");

        JoinedAt ??= now ?? DateTime.UtcNow;
        LeftAt = null;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Leave(DateTime? now = null)
    {
        LeftAt = now ?? DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }
}