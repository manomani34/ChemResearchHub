using ChemResearchHub.Application.Meetings.DTOs;

namespace ChemResearchHub.Application.Meetings.Interfaces;

public interface IMeetingService
{
    Task<MeetingDashboardDto> GetDashboardAsync(
    string userId,
    CancellationToken cancellationToken = default);

    Task<List<MeetingSessionListItemDto>> GetAllAsync(
    string userId,
    CancellationToken cancellationToken = default);

    Task<MeetingSessionDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        CreateMeetingSessionRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        UpdateMeetingSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> PublishAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> StartAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> PauseAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> ResumeAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<int?> AddAgendaItemAsync(
        int meetingSessionId,
        CreateAgendaItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAgendaItemAsync(
        int agendaItemId,
        UpdateAgendaItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAgendaItemAsync(
        int agendaItemId,
        CancellationToken cancellationToken = default);

    Task<bool> StartAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> SkipAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<bool> ExtendAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        int additionalSeconds,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<int?> AddParkingLotItemAsync(
        int meetingSessionId,
        CreateParkingLotRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<int?> AddDecisionAsync(
        int meetingSessionId,
        CreateMeetingDecisionRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<int?> AddActionItemAsync(
        int meetingSessionId,
        CreateMeetingActionItemRequest request,
        CancellationToken cancellationToken = default);

    Task<List<ParkingLotItemDto>> GetParkingLotItemsAsync(
    int meetingSessionId,
    CancellationToken cancellationToken = default);

    Task<bool> UpdateParkingLotItemAsync(
        int id,
        UpdateParkingLotRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> TriageParkingLotItemAsync(
        int id,
        TriageParkingLotRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ScheduleParkingLotItemAsync(
        int id,
        int targetMeetingSessionId,
        CancellationToken cancellationToken = default);

    Task<bool> AssignParkingLotItemAsync(
        int id,
        string assignedToUserId,
        CancellationToken cancellationToken = default);

    Task<bool> StartParkingLotReviewAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ResolveParkingLotItemAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> DismissParkingLotItemAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<MeetingDecisionDto>> GetDecisionsAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default);

    Task<List<MeetingActionItemDto>> GetActionItemsAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateActionItemAsync(
        int id,
        UpdateMeetingActionItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> StartActionItemAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> BlockActionItemAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteActionItemAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> CancelActionItemAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> LinkActionItemToWorkItemAsync(
        int id,
        int workItemId,
        CancellationToken cancellationToken = default);
}