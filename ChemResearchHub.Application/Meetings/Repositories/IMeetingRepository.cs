using ChemResearchHub.Domain.Entities.Meetings;

namespace ChemResearchHub.Application.Meetings.Repositories;


public interface IMeetingRepository
{
    Task<List<MeetingSession>> GetAllSessionsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<List<MeetingSession>> GetUpcomingSessionsAsync(
        string userId,
        DateTime from,
        int count,
        CancellationToken cancellationToken = default);

    Task<List<MeetingSession>> GetRecentSessionsAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default);

    Task<MeetingSession?> GetSessionByIdAsync(
        int id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    Task<MeetingSession?> GetSessionForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddSessionAsync(
        MeetingSession session,
        CancellationToken cancellationToken = default);

    Task<bool> SessionExistsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<MeetingSeries?> GetSeriesByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<AgendaItem?> GetAgendaItemByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ParkingLotItem?> GetParkingLotItemByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<MeetingDecision?> GetDecisionByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<MeetingActionItem?> GetActionItemByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<List<ParkingLotItem>> GetParkingLotItemsAsync(
    int meetingSessionId,
    CancellationToken cancellationToken = default);

    Task<List<MeetingDecision>> GetDecisionsAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default);

    Task<List<MeetingActionItem>> GetActionItemsAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default);

    Task<MeetingSession?> GetSessionForParkingLotAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default);
}