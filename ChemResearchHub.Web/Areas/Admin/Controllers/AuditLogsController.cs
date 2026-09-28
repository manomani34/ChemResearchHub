using ChemResearchHub.Application.AuditLogs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/AuditLogs")]
public class AuditLogsController : Controller
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? search,
        string? entityType,
        string? action,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var logs = await _service.GetAsync(
            search,
            entityType,
            action,
            page,
            50,
            cancellationToken);

        ViewBag.Search = search;
        ViewBag.EntityType = entityType;
        ViewBag.Action = action;
        ViewBag.Page = page;

        return View(logs);
    }
}
