using System.Security.Claims;
using ChemResearchHub.Domain.Entities.Notification;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Web.Areas.User.Controllers;

[Area("User")]
[Authorize]
[Route("User/Notifications")]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _db;

    public NotificationController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        const int pageSize = 20;

        var query = _db.Set<Notification>()
            .AsNoTracking()
            .Where(x => x.UserId == CurrentUserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Type,
                x.Title,
                x.Message,
                x.ActionUrl,
                x.CreatedAt,
                x.ReadAt,
                x.IsRead
            })
            .ToListAsync(cancellationToken);

        return Json(new
        {
            totalCount,
            page,
            pageSize,
            notifications
        });
    }

    [HttpGet("all")]
    public IActionResult All()
    {
        return View("~/Areas/User/Views/Notification/Index.cshtml");
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(
        CancellationToken cancellationToken)
    {
        var count = await _db.Set<Notification>()
            .CountAsync(
                x => x.UserId == CurrentUserId && x.ReadAt == null,
                cancellationToken);

        return Json(new { count });
    }

    [HttpPost("{id:int}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
    {
        var notification = await _db.Set<Notification>()
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserId == CurrentUserId,
                cancellationToken);

        if (notification is null)
            return NotFound();

        notification.MarkAsRead();

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true });
    }

    [HttpPost("read-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var unread = await _db.Set<Notification>()
            .Where(x =>
                x.UserId == CurrentUserId &&
                x.ReadAt == null)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
            notification.MarkAsRead();

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            success = true,
            count = unread.Count
        });
    }
}