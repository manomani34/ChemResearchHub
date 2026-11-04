using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Application.Meetings.Interfaces;
using ChemResearchHub.Application.Meetings.Repositories;
using ChemResearchHub.Domain.Entities.Meetings;
using ChemResearchHub.Domain.Enums.Meetings;
using ChemResearchHub.Application.WorkItems.Repositories;

namespace ChemResearchHub.Application.Meetings.Services;

public class MeetingService : IMeetingService
{
    private readonly IMeetingRepository _repository;
    private readonly IWorkItemRepository _workItemRepository;

    public MeetingService(
        IMeetingRepository repository,
        IWorkItemRepository workItemRepository)
    {
        _repository = repository;
        _workItemRepository = workItemRepository;
    }

    public async Task<MeetingActionItemDto?> GetActionItemByWorkItemIdAsync(
    int workItemId,
    CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
            return null;

        var actionItem =
            await _repository.GetActionItemByWorkItemIdAsync(
                workItemId,
                cancellationToken);

        return actionItem is null
            ? null
            : MapActionItem(actionItem);
    }

    public async Task<bool> UnlinkActionItemFromWorkItemAsync(
        int actionItemId,
        CancellationToken cancellationToken = default)
    {
        if (actionItemId <= 0)
            return false;

        var actionItem =
            await _repository.GetActionItemByIdAsync(
                actionItemId,
                cancellationToken);

        if (actionItem is null)
            return false;

        actionItem.UnlinkWorkItem();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }    

    public async Task<bool> LinkActionItemToWorkItemAsync(
    int id,
    int workItemId,
    CancellationToken cancellationToken = default)
    {
        if (id <= 0 || workItemId <= 0)
            return false;

        var actionItem = await _repository.GetActionItemByIdAsync(
            id,
            cancellationToken);

        if (actionItem is null)
            return false;

        var workItem = await _workItemRepository.GetByIdAsync(
            workItemId,
            cancellationToken);

        if (workItem is null)
            return false;

        var existingLink =
            await _repository.GetActionItemByWorkItemIdAsync(
                workItemId,
                cancellationToken);

        if (existingLink is not null && existingLink.Id != id)
        {
            throw new InvalidOperationException(
                "این Work Item قبلاً به یک Action Item دیگر متصل شده است.");
        }

        actionItem.LinkWorkItem(workItemId);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<MeetingDashboardDto> GetDashboardAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var now = DateTime.UtcNow;

        // فقط جلساتی که کاربر ایجاد کرده یا عضو فعال آن‌هاست.
        var sessions = await _repository.GetAllSessionsAsync(
            userId,
            cancellationToken);

        var upcoming = sessions
            .Where(x =>
                x.ScheduledStartAt >= now &&
                x.Status != MeetingSessionStatus.Completed &&
                x.Status != MeetingSessionStatus.Cancelled)
            .OrderBy(x => x.ScheduledStartAt)
            .Take(5)
            .Select(MapListItem)
            .ToList();

        var recent = sessions
            .Where(x =>
                x.ActualStartedAt.HasValue ||
                x.Status == MeetingSessionStatus.Completed)
            .OrderByDescending(x => x.ScheduledStartAt)
            .Take(5)
            .Select(MapListItem)
            .ToList();

        return new MeetingDashboardDto
        {
            TotalMeetings = sessions.Count,

            UpcomingMeetings = sessions.Count(x =>
                x.ScheduledStartAt >= now &&
                x.Status != MeetingSessionStatus.Completed &&
                x.Status != MeetingSessionStatus.Cancelled),

            CompletedMeetings = sessions.Count(x =>
                x.Status == MeetingSessionStatus.Completed),

            InProgressMeetings = sessions.Count(x =>
                x.Status == MeetingSessionStatus.InProgress),

            Upcoming = upcoming,
            Recent = recent
        };
    }

    public async Task<List<MeetingSessionListItemDto>> GetAllAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var sessions = await _repository.GetAllSessionsAsync(
            userId,
            cancellationToken);

        return sessions
            .Select(MapListItem)
            .ToList();
    }


    public async Task<MeetingSessionDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetSessionByIdAsync(
            id,
            true,
            cancellationToken);

        return session is null ? null : MapSession(session);
    }

    public async Task<int> CreateAsync(
        CreateMeetingSessionRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        if (request.MeetingSeriesId.HasValue)
        {
            var series = await _repository.GetSeriesByIdAsync(
                request.MeetingSeriesId.Value,
                cancellationToken);

            if (series is null)
                throw new ArgumentException(
                    "Meeting series was not found.");
        }

        var session = new MeetingSession(
            request.Title,
            request.ScheduledStartAt,
            request.PlannedDurationMinutes,
            request.MeetingSeriesId,
            request.Description);

        session.SetCreatedBy(currentUserId);

        session.SetFacilitator(
            string.IsNullOrWhiteSpace(request.FacilitatorUserId)
                ? currentUserId
                : request.FacilitatorUserId);

        await _repository.AddSessionAsync(
            session,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return session.Id;
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateMeetingSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetSessionForUpdateAsync(
            id,
            cancellationToken);

        if (session is null)
            return false;

        session.Update(
            request.Title,
            request.Description,
            request.ScheduledStartAt,
            request.PlannedDurationMinutes);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> PublishAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetSessionForUpdateAsync(
            id,
            cancellationToken);

        if (session is null)
            return false;

        session.Publish();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            id,
            cancellationToken);

        if (session is null)
            return false;

        session.Start();

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.MeetingStarted,
            currentUserId));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> PauseAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            id,
            cancellationToken);

        if (session is null)
            return false;

        session.Pause();

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.MeetingPaused,
            currentUserId));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ResumeAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            id,
            cancellationToken);

        if (session is null)
            return false;

        session.Resume();

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.MeetingResumed,
            currentUserId));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAsync(
        int id,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            id,
            cancellationToken);

        if (session is null)
            return false;

        session.Complete();

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.MeetingEnded,
            currentUserId));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int?> AddAgendaItemAsync(
        int meetingSessionId,
        CreateAgendaItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return null;

        if (session.Status != MeetingSessionStatus.Draft &&
            session.Status != MeetingSessionStatus.Published)
        {
            throw new InvalidOperationException(
                "Agenda cannot be changed after the meeting has started.");
        }

        var nextOrder = session.AgendaItems.Count == 0
            ? 0
            : session.AgendaItems.Max(x => x.SortOrder) + 1;

        var item = new AgendaItem(
            meetingSessionId,
            request.Title,
            request.DurationMinutes,
            nextOrder,
            request.Type,
            request.Priority,
            request.Description,
            request.PresenterUserId,
            request.ProjectId,
            request.ExpectedOutcome);

        session.AgendaItems.Add(item);

        await _repository.SaveChangesAsync(cancellationToken);

        return item.Id;
    }

    public async Task<bool> UpdateAgendaItemAsync(
        int agendaItemId,
        UpdateAgendaItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetAgendaItemByIdAsync(
            agendaItemId,
            cancellationToken);

        if (item is null)
            return false;

        item.Update(
            request.Title,
            request.Description,
            request.DurationMinutes,
            request.Type,
            request.Priority,
            request.PresenterUserId,
            request.ProjectId,
            request.ExpectedOutcome,
            request.SortOrder);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAgendaItemAsync(
        int agendaItemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetAgendaItemByIdAsync(
            agendaItemId,
            cancellationToken);

        if (item is null)
            return false;

        if (item.MeetingSession.Status != MeetingSessionStatus.Draft &&
            item.MeetingSession.Status != MeetingSessionStatus.Published)
        {
            throw new InvalidOperationException(
                "Agenda cannot be changed after the meeting has started.");
        }

        item.MeetingSession.AgendaItems.Remove(item);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return false;

        if (session.Status != MeetingSessionStatus.InProgress)
            throw new InvalidOperationException(
                "Meeting is not in progress.");

        if (session.TimerState != MeetingTimerState.Running)
        {
            throw new InvalidOperationException(
                "Agenda item cannot be started while the meeting timer is paused.");
        }

        var item = session.AgendaItems
            .FirstOrDefault(x => x.Id == agendaItemId);

        if (item is null)
            return false;

        if (session.AgendaItems.Any(x =>
                x.Id != agendaItemId &&
                (x.Status == AgendaItemStatus.InProgress ||
                 x.Status == AgendaItemStatus.Overtime)))
        {
            throw new InvalidOperationException(
                "Another agenda item is still active. Complete or skip it first.");
        }

        item.Start();

        var orderedItems = session.AgendaItems
            .OrderBy(x => x.SortOrder)
            .ToList();

        session.SetCurrentAgendaItemIndex(
            orderedItems.FindIndex(x => x.Id == item.Id));

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.SectionStarted,
            currentUserId,
            item.Id));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return false;

        var item = session.AgendaItems
            .FirstOrDefault(x => x.Id == agendaItemId);

        if (item is null)
            return false;

        item.Complete();

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.SectionCompleted,
            currentUserId,
            item.Id));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SkipAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return false;

        var item = session.AgendaItems
            .FirstOrDefault(x => x.Id == agendaItemId);

        if (item is null)
            return false;

        item.Skip();

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.SectionSkipped,
            currentUserId,
            item.Id));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExtendAgendaItemAsync(
        int meetingSessionId,
        int agendaItemId,
        int additionalSeconds,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return false;

        var item = session.AgendaItems
            .FirstOrDefault(x => x.Id == agendaItemId);

        if (item is null)
            return false;

        item.Extend(additionalSeconds);

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.SectionExtended,
            currentUserId,
            item.Id,
            additionalSeconds));

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int?> AddParkingLotItemAsync(
        int meetingSessionId,
        CreateParkingLotRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return null;

        var item = new ParkingLotItem(
            meetingSessionId,
            request.Title,
            request.Summary,
            currentUserId,
            request.AgendaItemId,
            request.ProjectId);

        session.ParkingLotItems.Add(item);

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.ParkingLotCreated,
            currentUserId,
            request.AgendaItemId,
            description: request.Title));

        await _repository.SaveChangesAsync(cancellationToken);

        return item.Id;
    }

    public async Task<int?> AddDecisionAsync(
        int meetingSessionId,
        CreateMeetingDecisionRequest request,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(currentUserId);

        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return null;

        var decision = new MeetingDecision(
            meetingSessionId,
            request.Title,
            request.Description,
            currentUserId,
            request.AgendaItemId,
            request.ProjectId);

        session.Decisions.Add(decision);

        session.TimerEvents.Add(new MeetingTimerEvent(
            session.Id,
            MeetingTimerEventType.DecisionCreated,
            currentUserId,
            request.AgendaItemId,
            description: request.Title));

        await _repository.SaveChangesAsync(cancellationToken);

        return decision.Id;
    }

    public async Task<int?> AddActionItemAsync(
    int meetingSessionId,
    CreateMeetingActionItemRequest request,
    CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetSessionForUpdateAsync(
            meetingSessionId,
            cancellationToken);

        if (session is null)
            return null;

        var actionItem = new MeetingActionItem(
            meetingSessionId,
            request.Title,
            request.Description,
            request.AssignedToUserId,
            request.DueDate,
            request.ProjectId,
            request.DecisionId,
            request.AcceptanceCriteria);

        if (request.WorkItemId.HasValue)
        {
            var workItem = await _workItemRepository.GetByIdAsync(
                request.WorkItemId.Value,
                cancellationToken);

            if (workItem is null)
                throw new InvalidOperationException(
                    "Work Item انتخاب‌شده وجود ندارد.");

            var existingLink =
                await _repository.GetActionItemByWorkItemIdAsync(
                    request.WorkItemId.Value,
                    cancellationToken);

            if (existingLink is not null)
                throw new InvalidOperationException(
                    "این Work Item قبلاً به یک Action Item دیگر متصل شده است.");

            actionItem.LinkWorkItem(request.WorkItemId.Value);
        }

        session.ActionItems.Add(actionItem);

        await _repository.SaveChangesAsync(cancellationToken);

        return actionItem.Id;
    }

    private static MeetingSessionListItemDto MapListItem(
        MeetingSession session)
    {
        return new MeetingSessionListItemDto
        {
            Id = session.Id,
            Title = session.Title,
            Description = session.Description,
            ScheduledStartAt = session.ScheduledStartAt,
            PlannedDurationMinutes = session.PlannedDurationMinutes,
            Status = session.Status,
            AgendaItemCount = session.AgendaItems.Count,
            FacilitatorUserId = session.FacilitatorUserId
        };
    }

    private static MeetingSessionDto MapSession(
        MeetingSession session)
    {
        var now = DateTime.UtcNow;

        return new MeetingSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            Description = session.Description,
            MeetingSeriesId = session.MeetingSeriesId,
            ScheduledStartAt = session.ScheduledStartAt,
            PlannedDurationMinutes = session.PlannedDurationMinutes,
            ActualStartedAt = session.ActualStartedAt,
            ActualEndedAt = session.ActualEndedAt,
            Status = session.Status,
            TimerState = session.TimerState,
            FacilitatorUserId = session.FacilitatorUserId,
            CreatedByUserId = session.CreatedByUserId,
            CurrentAgendaItemIndex = session.CurrentAgendaItemIndex,
            ElapsedSeconds = session.GetElapsedSeconds(now),
            RemainingSeconds = session.GetRemainingSeconds(now),

            AgendaItems = session.AgendaItems
                .OrderBy(x => x.SortOrder)
                .Select(x => new AgendaItemDto
                {
                    Id = x.Id,
                    MeetingSessionId = x.MeetingSessionId,
                    Title = x.Title,
                    Description = x.Description,
                    Type = x.Type,
                    Priority = x.Priority,
                    Status = x.Status,
                    DurationMinutes = x.DurationMinutes,
                    SortOrder = x.SortOrder,
                    PresenterUserId = x.PresenterUserId,
                    ProjectId = x.ProjectId,
                    ExpectedOutcome = x.ExpectedOutcome,
                    ActualStartedAt = x.ActualStartedAt,
                    ActualEndedAt = x.ActualEndedAt,
                    ExtendedSeconds = x.ExtendedSeconds
                })
                .ToList()
        };
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "Current user id is required.",
                nameof(userId));
    }

    public async Task<List<ParkingLotItemDto>> GetParkingLotItemsAsync(
    int meetingSessionId,
    CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetParkingLotItemsAsync(
            meetingSessionId,
            cancellationToken);

        return items.Select(x => new ParkingLotItemDto
        {
            Id = x.Id,
            MeetingSessionId = x.MeetingSessionId,
            Title = x.Title,
            Summary = x.Summary,
            RaisedByUserId = x.RaisedByUserId,
            AgendaItemId = x.AgendaItemId,
            ProjectId = x.ProjectId,
            Status = x.Status,
            Priority = x.Priority,
            AssignedToUserId = x.AssignedToUserId,
            TargetMeetingSessionId = x.TargetMeetingSessionId,
            CreatedAt = x.CreatedAt,
            ModifiedAt = x.ModifiedAt
        }).ToList();
    }

    public async Task<bool> UpdateParkingLotItemAsync(
        int id,
        UpdateParkingLotRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetParkingLotItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Update(
            request.Title,
            request.Summary,
            request.ProjectId);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> TriageParkingLotItemAsync(
        int id,
        TriageParkingLotRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetParkingLotItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Triage(
            request.Priority,
            request.AssignedToUserId);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ScheduleParkingLotItemAsync(
        int id,
        int targetMeetingSessionId,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetParkingLotItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        var targetSession = await _repository.GetSessionForParkingLotAsync(
            targetMeetingSessionId,
            cancellationToken);

        if (targetSession is null)
            throw new ArgumentException("Target meeting was not found.");

        item.Schedule(targetMeetingSessionId);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> AssignParkingLotItemAsync(
        int id,
        string assignedToUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assignedToUserId))
            throw new ArgumentException("Assigned user is required.");

        var item = await _repository.GetParkingLotItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Assign(assignedToUserId);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartParkingLotReviewAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetParkingLotItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.StartReview();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ResolveParkingLotItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetParkingLotItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Resolve();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DismissParkingLotItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetParkingLotItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Dismiss();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<MeetingDecisionDto>> GetDecisionsAsync(
    int meetingSessionId,
    CancellationToken cancellationToken = default)
    {
        var decisions = await _repository.GetDecisionsAsync(
            meetingSessionId,
            cancellationToken);

        return decisions.Select(x => new MeetingDecisionDto
        {
            Id = x.Id,
            MeetingSessionId = x.MeetingSessionId,
            Title = x.Title,
            Description = x.Description,
            DecidedByUserId = x.DecidedByUserId,
            AgendaItemId = x.AgendaItemId,
            ProjectId = x.ProjectId,
            Status = x.Status,
            SupersededByDecisionId = x.SupersededByDecisionId,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<List<MeetingActionItemDto>> GetActionItemsAsync(
    int meetingSessionId,
    CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetActionItemsAsync(
            meetingSessionId,
            cancellationToken);

        return items.Select(MapActionItem).ToList();
    }

    public async Task<bool> UpdateActionItemAsync(
    int id,
    UpdateMeetingActionItemRequest request,
    CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetActionItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Update(
            request.Title,
            request.Description,
            request.AssignedToUserId,
            request.DueDate,
            request.ProjectId,
            request.DecisionId,
            request.AcceptanceCriteria);

        if (item.Type == MeetingActionItemType.ConvertedWorkItem)
        {
            if (request.WorkItemId != item.WorkItemId)
            {
                throw new InvalidOperationException(
                    "اتصال Action Item تبدیل‌شده به Work Item قابل تغییر نیست.");
            }
        }
        else if (request.WorkItemId.HasValue)
        {
            var workItem = await _workItemRepository.GetByIdAsync(
                request.WorkItemId.Value,
                cancellationToken);

            if (workItem is null)
                throw new InvalidOperationException(
                    "Work Item انتخاب‌شده وجود ندارد.");

            var existingLink =
                await _repository.GetActionItemByWorkItemIdAsync(
                    request.WorkItemId.Value,
                    cancellationToken);

            if (existingLink is not null && existingLink.Id != id)
                throw new InvalidOperationException(
                    "این Work Item قبلاً به یک Action Item دیگر متصل شده است.");

            item.LinkWorkItem(request.WorkItemId.Value);
        }
        else
        {
            item.UnlinkWorkItem();
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartActionItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetActionItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Start();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> BlockActionItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetActionItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Block();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteActionItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetActionItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Complete();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelActionItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetActionItemByIdAsync(
            id,
            cancellationToken);

        if (item is null)
            return false;

        item.Cancel();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static MeetingActionItemDto MapActionItem(
        MeetingActionItem item)
    {
        return new MeetingActionItemDto
        {
            Id = item.Id,
            MeetingSessionId = item.MeetingSessionId,
            Title = item.Title,
            Description = item.Description,
            AssignedToUserId = item.AssignedToUserId,
            DueDate = item.DueDate,
            ProjectId = item.ProjectId,
            MeetingDecisionId = item.MeetingDecisionId,
            AcceptanceCriteria = item.AcceptanceCriteria,
            Status = item.Status,
            Type = item.Type,
            WorkItemId = item.WorkItemId,
            CompletedAt = item.CompletedAt,
            CreatedAt = item.CreatedAt
        };
    }
}