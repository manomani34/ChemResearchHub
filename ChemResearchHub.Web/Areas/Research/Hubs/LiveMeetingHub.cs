using System.Security.Claims;
using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Application.Meetings.Interfaces;
using ChemResearchHub.Application.Meetings.Repositories;
using ChemResearchHub.Domain.Entities.Meetings;
using ChemResearchHub.Domain.Enums.Meetings;
using ChemResearchHub.Web.Areas.Research.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChemResearchHub.Web.Areas.Research.Hubs;

[Authorize]
public class LiveMeetingHub : Hub
{
    public const string HubUrl = "/hubs/research-meetings";

    private readonly IMeetingService _meetingService;
    private readonly IMeetingRepository _meetingRepository;

    public LiveMeetingHub(
        IMeetingService meetingService,
        IMeetingRepository meetingRepository)
    {
        _meetingService = meetingService;
        _meetingRepository = meetingRepository;
    }

    //private async Task BroadcastSnapshotAsync(
    //int meetingSessionId,
    //LiveMeetingSnapshotDto snapshot)
    //{
    //    var session = await _meetingRepository.GetSessionByIdAsync(
    //        meetingSessionId,
    //        true,
    //        Context.ConnectionAborted);

    //    if (session is null)
    //        return;

    //    var groupName = GetGroupName(meetingSessionId);

    //    var authorizedUserIds = session.Participants
    //        .Where(x => x.IsActive)
    //        .Select(x => x.UserId)
    //        .Append(session.FacilitatorUserId)
    //        .Where(x => !string.IsNullOrWhiteSpace(x))
    //        .Distinct(StringComparer.Ordinal)
    //        .ToHashSet(StringComparer.Ordinal);

    //    // اتصال‌های داخل Group را بررسی می‌کنیم.
    //    // فقط کاربران دارای عضویت فعال مجاز به دریافت پیام هستند.
    //    var currentUserId = GetCurrentUserId();

    //    if (authorizedUserIds.Contains(currentUserId))
    //    {
    //        await Clients.Group(groupName)
    //            .SendAsync(
    //                "MeetingSnapshotUpdated",
    //                snapshot,
    //                Context.ConnectionAborted);
    //    }
    //}

    public async Task<LiveMeetingSnapshotDto> JoinMeeting(
    int meetingSessionId)
    {
        await GetAuthorizedSessionAsync(
            meetingSessionId,
            requireControl: false);

        return await CreateSnapshotAsync(meetingSessionId);
    }

    public async Task<LiveMeetingSnapshotDto> GetSnapshot(
        int meetingSessionId)
    {
        await GetAuthorizedSessionAsync(
            meetingSessionId,
            requireControl: false);

        return await CreateSnapshotAsync(meetingSessionId);
    }

    public Task<LiveMeetingSnapshotDto> StartMeetingTimer(
        int meetingSessionId)
    {
        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.StartAsync(
                    meetingSessionId,
                    userId,
                    ct));
    }

    public Task<LiveMeetingSnapshotDto> PauseMeetingTimer(
        int meetingSessionId)
    {
        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.PauseAsync(
                    meetingSessionId,
                    userId,
                    ct));
    }

    public Task<LiveMeetingSnapshotDto> ResumeMeetingTimer(
        int meetingSessionId)
    {
        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.ResumeAsync(
                    meetingSessionId,
                    userId,
                    ct));
    }

    public Task<LiveMeetingSnapshotDto> CompleteMeetingTimer(
        int meetingSessionId)
    {
        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.CompleteAsync(
                    meetingSessionId,
                    userId,
                    ct));
    }

    public Task<LiveMeetingSnapshotDto> StartAgendaItem(
        int meetingSessionId,
        int agendaItemId)
    {
        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.StartAgendaItemAsync(
                    meetingSessionId,
                    agendaItemId,
                    userId,
                    ct));
    }

    public Task<LiveMeetingSnapshotDto> CompleteAgendaItem(
        int meetingSessionId,
        int agendaItemId)
    {
        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.CompleteAgendaItemAsync(
                    meetingSessionId,
                    agendaItemId,
                    userId,
                    ct));
    }

    public Task<LiveMeetingSnapshotDto> SkipAgendaItem(
        int meetingSessionId,
        int agendaItemId)
    {
        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.SkipAgendaItemAsync(
                    meetingSessionId,
                    agendaItemId,
                    userId,
                    ct));
    }

    public Task<LiveMeetingSnapshotDto> ExtendAgendaItem(
        int meetingSessionId,
        int agendaItemId,
        int additionalSeconds)
    {
        if (additionalSeconds <= 0 ||
            additionalSeconds > 3600)
        {
            throw new HubException(
                "Extension must be between 1 and 3600 seconds.");
        }

        return ExecuteControlAsync(
            meetingSessionId,
            (userId, ct) =>
                _meetingService.ExtendAgendaItemAsync(
                    meetingSessionId,
                    agendaItemId,
                    additionalSeconds,
                    userId,
                    ct));
    }

    private async Task<LiveMeetingSnapshotDto> ExecuteControlAsync(
    int meetingSessionId,
    Func<string, CancellationToken, Task<bool>> operation)
    {
        await GetAuthorizedSessionAsync(
            meetingSessionId,
            requireControl: true);

        var userId = GetCurrentUserId();

        var succeeded = await operation(
            userId,
            Context.ConnectionAborted);

        if (!succeeded)
        {
            throw new HubException(
                "The requested meeting operation could not be completed.");
        }

        var snapshot = await CreateSnapshotAsync(meetingSessionId);

        await BroadcastSnapshotAsync(
            meetingSessionId,
            snapshot);

        return snapshot;
    }

    private async Task<MeetingSession> GetAuthorizedSessionAsync(
        int meetingSessionId,
        bool requireControl)
    {
        if (meetingSessionId <= 0)
            throw new HubException("Invalid meeting id.");

        var userId = GetCurrentUserId();

        var session = await _meetingRepository.GetSessionByIdAsync(
            meetingSessionId,
            true,
            Context.ConnectionAborted);

        if (session is null)
            throw new HubException("Meeting was not found.");

        if (!MeetingRoomAccess.CanJoin(session, userId))
        {
            throw new HubException(
                "You are not an active participant of this meeting.");
        }

        if (requireControl &&
            !MeetingRoomAccess.CanControl(session, userId))
        {
            throw new HubException(
                "You are not allowed to control this meeting.");
        }

        return session;
    }

    private async Task<LiveMeetingSnapshotDto> CreateSnapshotAsync(
        int meetingSessionId)
    {
        // Entity جلسه را برای دسترسی به TimerEvents دریافت می‌کنیم.
        var session = await _meetingRepository.GetSessionByIdAsync(
            meetingSessionId,
            true,
            Context.ConnectionAborted);

        if (session is null)
            throw new HubException("Meeting was not found.");

        var meeting = await _meetingService.GetByIdAsync(
            meetingSessionId,
            Context.ConnectionAborted);

        if (meeting is null)
            throw new HubException("Meeting was not found.");

        var now = DateTime.UtcNow;

        var agendaItems = meeting.AgendaItems
            .OrderBy(x => x.SortOrder)
            .ToList();

        var currentItem = agendaItems.FirstOrDefault(x =>
            x.Status == AgendaItemStatus.InProgress ||
            x.Status == AgendaItemStatus.Overtime);

        var currentDomainItem = currentItem is null
            ? null
            : session.AgendaItems.FirstOrDefault(
                x => x.Id == currentItem.Id);

        int? currentRemainingSeconds = null;
        int? currentDurationSeconds = null;

        if (currentItem is not null &&
            currentDomainItem is not null)
        {
            currentDurationSeconds =
                (currentDomainItem.DurationMinutes * 60) +
                currentDomainItem.ExtendedSeconds;

            currentRemainingSeconds =
                CalculateAgendaItemRemainingSeconds(
                    session,
                    currentDomainItem,
                    now);
        }

        var liveAgendaItems = agendaItems.Select(item =>
        {
            var domainItem = session.AgendaItems
                .FirstOrDefault(x => x.Id == item.Id);

            int? remainingSeconds = null;

            if (domainItem is not null &&
                domainItem.ActualStartedAt.HasValue)
            {
                remainingSeconds =
                    CalculateAgendaItemRemainingSeconds(
                        session,
                        domainItem,
                        now);
            }

            return new LiveAgendaItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Status = item.Status.ToString(),
                Type = item.Type.ToString(),
                Priority = item.Priority.ToString(),
                DurationMinutes = item.DurationMinutes,
                ExtendedSeconds = item.ExtendedSeconds,
                SortOrder = item.SortOrder,
                PresenterUserId = item.PresenterUserId,
                ActualStartedAt = item.ActualStartedAt,
                ActualEndedAt = item.ActualEndedAt,
                RemainingSeconds = remainingSeconds
            };
        }).ToList();

        return new LiveMeetingSnapshotDto
        {
            MeetingSessionId = meeting.Id,
            Title = meeting.Title,
            Description = meeting.Description,

            MeetingStatus = meeting.Status.ToString(),
            TimerState = meeting.TimerState.ToString(),

            ServerTimeUtc = now,
            ActualStartedAt = meeting.ActualStartedAt,
            ActualEndedAt = meeting.ActualEndedAt,

            PlannedDurationMinutes =
                meeting.PlannedDurationMinutes,

            ElapsedSeconds = meeting.ElapsedSeconds,
            RemainingSeconds = meeting.RemainingSeconds,

            CurrentAgendaItemIndex =
                meeting.CurrentAgendaItemIndex,

            CurrentAgendaItemId = currentItem?.Id,
            CurrentAgendaItemTitle = currentItem?.Title,
            CurrentAgendaItemStatus =
                currentItem?.Status.ToString(),

            CurrentAgendaItemRemainingSeconds =
                currentRemainingSeconds,

            CurrentAgendaItemDurationSeconds =
                currentDurationSeconds,

            AgendaItems = liveAgendaItems
        };
    }

    private static int? CalculateAgendaItemRemainingSeconds(
        MeetingSession session,
        AgendaItem item,
        DateTime nowUtc)
    {
        if (!item.ActualStartedAt.HasValue)
            return null;

        var startAt = EnsureUtc(item.ActualStartedAt.Value);

        // برای بخش تمام‌شده از زمان پایان خود بخش استفاده می‌کنیم.
        // اگر جلسه تمام شده ولی بخش پایان نیافته، زمان پایان جلسه ملاک است.
        var endAt = item.ActualEndedAt.HasValue
            ? EnsureUtc(item.ActualEndedAt.Value)
            : session.ActualEndedAt.HasValue
                ? EnsureUtc(session.ActualEndedAt.Value)
                : nowUtc;

        if (endAt < startAt)
            endAt = startAt;

        var wallClockSeconds =
            (endAt - startAt).TotalSeconds;

        var pausedSeconds = CalculatePausedSeconds(
            session.TimerEvents,
            startAt,
            endAt);

        var activeSeconds = Math.Max(
            0,
            wallClockSeconds - pausedSeconds);

        var allowedSeconds =
            (item.DurationMinutes * 60) +
            item.ExtendedSeconds;

        return (int)Math.Ceiling(
            allowedSeconds - activeSeconds);
    }

    private static double CalculatePausedSeconds(
        IEnumerable<MeetingTimerEvent> timerEvents,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc)
    {
        if (rangeEndUtc <= rangeStartUtc)
            return 0;

        var events = timerEvents
            .Where(x =>
                x.EventType == MeetingTimerEventType.MeetingPaused ||
                x.EventType == MeetingTimerEventType.MeetingResumed ||
                x.EventType == MeetingTimerEventType.MeetingEnded)
            .OrderBy(x => x.OccurredAt)
            .ToList();

        DateTime? pauseStartedAt = null;
        double totalPausedSeconds = 0;

        foreach (var timerEvent in events)
        {
            var occurredAt = EnsureUtc(timerEvent.OccurredAt);

            if (occurredAt > rangeEndUtc)
                break;

            switch (timerEvent.EventType)
            {
                case MeetingTimerEventType.MeetingPaused:
                    pauseStartedAt ??= occurredAt;
                    break;

                case MeetingTimerEventType.MeetingResumed:
                case MeetingTimerEventType.MeetingEnded:

                    if (pauseStartedAt.HasValue)
                    {
                        totalPausedSeconds += GetOverlappingSeconds(
                            pauseStartedAt.Value,
                            occurredAt,
                            rangeStartUtc,
                            rangeEndUtc);

                        pauseStartedAt = null;
                    }

                    break;
            }
        }

        // اگر در انتهای بازه هنوز Pause باز باشد،
        // آن را تا انتهای بازه محاسبه می‌کنیم.
        if (pauseStartedAt.HasValue)
        {
            totalPausedSeconds += GetOverlappingSeconds(
                pauseStartedAt.Value,
                rangeEndUtc,
                rangeStartUtc,
                rangeEndUtc);
        }

        return totalPausedSeconds;
    }

    private static double GetOverlappingSeconds(
        DateTime intervalStart,
        DateTime intervalEnd,
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        var overlapStart = intervalStart > rangeStart
            ? intervalStart
            : rangeStart;

        var overlapEnd = intervalEnd < rangeEnd
            ? intervalEnd
            : rangeEnd;

        if (overlapEnd <= overlapStart)
            return 0;

        return (overlapEnd - overlapStart).TotalSeconds;
    }

    private string GetCurrentUserId()
    {
        var userId = Context.User?.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException(
                "Authenticated user id was not found.");
        }

        return userId;
    }

    //private static string GetGroupName(int meetingSessionId)
    //    => $"meeting:{meetingSessionId}";

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc)
        };
    }

    private async Task BroadcastSnapshotAsync(
    int meetingSessionId,
    LiveMeetingSnapshotDto snapshot)
    {
        var session = await _meetingRepository.GetSessionByIdAsync(
            meetingSessionId,
            true,
            Context.ConnectionAborted);

        if (session is null)
            return;

        var authorizedUserIds = session.Participants
            .Where(x => x.IsActive)
            .Select(x => x.UserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Append(session.FacilitatorUserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (authorizedUserIds.Count == 0)
            return;

        await Clients.Users(authorizedUserIds)
            .SendAsync(
                "MeetingSnapshotUpdated",
                snapshot,
                Context.ConnectionAborted);
    }
}