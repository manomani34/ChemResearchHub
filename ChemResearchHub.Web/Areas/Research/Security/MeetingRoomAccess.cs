
using ChemResearchHub.Domain.Entities.Meetings;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Web.Areas.Research.Security;

internal static class MeetingRoomAccess
{
    /// <summary>
    /// Determines whether the user can view meeting details.
    /// The creator can view the meeting even if not a participant.
    /// </summary>
    public static bool CanView(MeetingSession session, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return IsCreator(session, userId) ||
               CanJoin(session, userId);
    }

    /// <summary>
    /// Determines whether the user can enter the live meeting room.
    /// </summary>
    public static bool CanJoin(MeetingSession session, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (IsFacilitator(session, userId))
            return true;

        return session.Participants.Any(x =>
            x.UserId == userId && x.IsActive);
    }

    /// <summary>
    /// Determines whether the user can control the meeting.
    /// </summary>
    public static bool CanControl(MeetingSession session, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (IsFacilitator(session, userId))
            return true;

        return session.Participants.Any(x =>
            x.UserId == userId &&
            x.IsActive &&
            (x.Role == MeetingParticipantRole.Facilitator ||
             x.Role == MeetingParticipantRole.CoFacilitator ||
             x.Role == MeetingParticipantRole.Admin));
    }

    public static MeetingParticipantRole? GetRole(
        MeetingSession session,
        string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        if (IsFacilitator(session, userId))
            return MeetingParticipantRole.Facilitator;

        return session.Participants
            .FirstOrDefault(x =>
                x.UserId == userId && x.IsActive)
            ?.Role;
    }

    private static bool IsCreator(
        MeetingSession session,
        string userId)
    {
        return string.Equals(
            session.CreatedByUserId,
            userId,
            StringComparison.Ordinal);
    }

    private static bool IsFacilitator(
        MeetingSession session,
        string userId)
    {
        return string.Equals(
            session.FacilitatorUserId,
            userId,
            StringComparison.Ordinal);
    }
}
