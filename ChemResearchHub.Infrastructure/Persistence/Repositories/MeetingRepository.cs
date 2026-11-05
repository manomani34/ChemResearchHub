using ChemResearchHub.Application.Meetings.Repositories;
using ChemResearchHub.Domain.Entities.Meetings;
using ChemResearchHub.Domain.Enums.Meetings;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<List<MeetingSession>> GetAllSessionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User id is required.",
                nameof(userId));

        userId = userId.Trim();

        return await _context.MeetingSessions
            .AsNoTracking()
            .Where(x =>
                x.CreatedByUserId == userId ||
                x.Participants.Any(p =>
                    p.UserId == userId &&
                    p.IsActive))
            .Include(x => x.AgendaItems)
            .OrderByDescending(x => x.ScheduledStartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MeetingSession>> GetUpcomingSessionsAsync(
        string userId,
        DateTime from,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User id is required.",
                nameof(userId));

        if (count <= 0)
            return new List<MeetingSession>();

        userId = userId.Trim();

        return await _context.MeetingSessions
            .AsNoTracking()
            .Where(x =>
                (x.CreatedByUserId == userId ||
                 x.Participants.Any(p =>
                     p.UserId == userId &&
                     p.IsActive)) &&
                x.ScheduledStartAt >= from &&
                x.Status != MeetingSessionStatus.Completed &&
                x.Status != MeetingSessionStatus.Cancelled)
            .Include(x => x.AgendaItems)
            .OrderBy(x => x.ScheduledStartAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MeetingSession>> GetRecentSessionsAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException(
                "User id is required.",
                nameof(userId));

        if (count <= 0)
            return new List<MeetingSession>();

        userId = userId.Trim();

        return await _context.MeetingSessions
            .AsNoTracking()
            .Where(x =>
                (x.CreatedByUserId == userId ||
                 x.Participants.Any(p =>
                     p.UserId == userId &&
                     p.IsActive)) &&
                (x.ActualStartedAt != null ||
                 x.Status == MeetingSessionStatus.Completed))
            .Include(x => x.AgendaItems)
            .OrderByDescending(x => x.ScheduledStartAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }


    public async Task<MeetingSession?> GetSessionByIdAsync(
        int id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MeetingSession> query =
            _context.MeetingSessions.AsNoTracking();

        if (includeDetails)
        {
            query = query
                .Include(x => x.AgendaItems)
                .Include(x => x.Participants)
                .Include(x => x.ParkingLotItems)
                .Include(x => x.Decisions)
                .Include(x => x.ActionItems)
                .Include(x => x.TimerEvents);
        }

        return await query.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
    }

    public async Task<MeetingSession?> GetSessionForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingSessions
            .Include(x => x.AgendaItems)
            .Include(x => x.Participants)
            .Include(x => x.ParkingLotItems)
            .Include(x => x.Decisions)
            .Include(x => x.ActionItems)
            .Include(x => x.TimerEvents)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddSessionAsync(
        MeetingSession session,
        CancellationToken cancellationToken = default)
    {
        await _context.MeetingSessions.AddAsync(
            session,
            cancellationToken);
    }

    public async Task<bool> SessionExistsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingSessions
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<MeetingSeries?> GetSeriesByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingSeries
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<AgendaItem?> GetAgendaItemByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingAgendaItems
            .Include(x => x.MeetingSession)
                .ThenInclude(x => x.AgendaItems)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<ParkingLotItem?> GetParkingLotItemByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingParkingLotItems
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<MeetingDecision?> GetDecisionByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingDecisions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<MeetingActionItem?> GetActionItemByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingActionItems
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<MeetingActionItem?> GetActionItemByWorkItemIdAsync(
    int workItemId,
    CancellationToken cancellationToken = default)
    {
        return await _context.MeetingActionItems
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.WorkItemId == workItemId,
                cancellationToken);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ParkingLotItem>> GetParkingLotItemsAsync(
    int meetingSessionId,
    CancellationToken cancellationToken = default)
    {
        return await _context.MeetingParkingLotItems
            .AsNoTracking()
            .Where(x => x.MeetingSessionId == meetingSessionId)
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.Priority)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MeetingDecision>> GetDecisionsAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingDecisions
            .AsNoTracking()
            .Where(x => x.MeetingSessionId == meetingSessionId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MeetingActionItem>> GetActionItemsAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingActionItems
            .AsNoTracking()
            .Where(x => x.MeetingSessionId == meetingSessionId)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<MeetingSession?> GetSessionForParkingLotAsync(
        int meetingSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MeetingSessions
            .Include(x => x.ParkingLotItems)
            .FirstOrDefaultAsync(
                x => x.Id == meetingSessionId,
                cancellationToken);
    }
}