
using System.Security.Claims;
using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Application.Meetings.Interfaces;
using ChemResearchHub.Application.Meetings.Repositories;
using ChemResearchHub.Domain.Entities.Meetings;
using ChemResearchHub.Domain.Enums.Meetings;
using ChemResearchHub.Web.Areas.Research.Security;
using ChemResearchHub.Web.Models.Meetings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.Research.Controllers;

[Area("Research")]
[Route("Meetings")]
[Authorize]
public class MeetingsController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly IMeetingRepository _meetingRepository;

    public MeetingsController(
        IMeetingService meetingService,
        IMeetingRepository meetingRepository)
    {
        _meetingService = meetingService;
        _meetingRepository = meetingRepository;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private async Task<(MeetingSession? Session, bool Forbidden)>
        GetMeetingAccessAsync(
            int meetingSessionId,
            bool requireControl,
            CancellationToken cancellationToken)
    {
        var session = await _meetingRepository.GetSessionByIdAsync(
            meetingSessionId,
            true,
            cancellationToken);

        if (session is null)
            return (null, false);

        var userId = CurrentUserId;

        // Creator can view the meeting he/she created.
        // Being the creator does not automatically grant control permission.
        if (!MeetingRoomAccess.CanView(session, userId))
            return (session, true);

        if (requireControl &&
            !MeetingRoomAccess.CanControl(session, userId))
        {
            return (session, true);
        }

        return (session, false);
    }

    private IActionResult AccessDeniedOrNotFound(
        MeetingSession? session,
        bool forbidden)
    {
        if (session is null)
            return NotFound();

        if (forbidden)
            return Forbid();

        return null!;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        var dashboard = await _meetingService.GetDashboardAsync(
            CurrentUserId,
            cancellationToken);

        return View(dashboard);
    }

    [HttpGet("All")]
    public async Task<IActionResult> All(
    CancellationToken cancellationToken)
    {
        var meetings = await _meetingService.GetAllAsync(
            CurrentUserId,
            cancellationToken);

        return View(meetings);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateMeetingViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateMeetingViewModel model,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId))
            return Challenge();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var request = new CreateMeetingSessionRequest
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                ScheduledStartAt = model.ScheduledStartAt,
                PlannedDurationMinutes = model.PlannedDurationMinutes,
                FacilitatorUserId = model.FacilitatorUserId
            };

            var id = await _meetingService.CreateAsync(
                request,
                CurrentUserId,
                cancellationToken);

            TempData["SuccessMessage"] =
                "جلسه پژوهشی با موفقیت ایجاد شد.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: false,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        var meeting = await _meetingService.GetByIdAsync(
            id,
            cancellationToken);

        if (meeting is null)
            return NotFound();

        return View(meeting);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        var meeting = await _meetingService.GetByIdAsync(
            id,
            cancellationToken);

        if (meeting is null)
            return NotFound();

        if (meeting.Status != MeetingSessionStatus.Draft &&
            meeting.Status != MeetingSessionStatus.Published)
        {
            TempData["ErrorMessage"] =
                "جلسه پس از شروع قابل ویرایش نیست.";

            return RedirectToAction(nameof(Details), new { id });
        }

        return View(new EditMeetingViewModel
        {
            Id = meeting.Id,
            Title = meeting.Title,
            Description = meeting.Description,
            ScheduledStartAt = meeting.ScheduledStartAt,
            PlannedDurationMinutes = meeting.PlannedDurationMinutes
        });
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EditMeetingViewModel model,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var updated = await _meetingService.UpdateAsync(
                id,
                new UpdateMeetingSessionRequest
                {
                    Title = model.Title.Trim(),
                    Description = model.Description?.Trim(),
                    ScheduledStartAt = model.ScheduledStartAt,
                    PlannedDurationMinutes = model.PlannedDurationMinutes
                },
                cancellationToken);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] =
                "اطلاعات جلسه به‌روزرسانی شد.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost("Publish/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.PublishAsync(
                id,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "دستور جلسه منتشر شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("Start/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.StartAsync(
                id,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] = "جلسه آغاز شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("Pause/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pause(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.PauseAsync(
                id,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] = "جلسه متوقف شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("Resume/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resume(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.ResumeAsync(
                id,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] = "جلسه ادامه یافت.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("Complete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(
        int id,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            id,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.CompleteAsync(
                id,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] = "جلسه پایان یافت.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{meetingSessionId:int}/Agenda/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAgendaItem(
        int meetingSessionId,
        CreateAgendaItemRequest model,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            meetingSessionId,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] =
                "اطلاعات بخش دستور جلسه معتبر نیست.";

            return RedirectToAction(
                nameof(Details),
                new { id = meetingSessionId });
        }

        try
        {
            await _meetingService.AddAgendaItemAsync(
                meetingSessionId,
                model,
                cancellationToken);

            TempData["SuccessMessage"] =
                "بخش جدید به دستور جلسه اضافه شد.";
        }
        catch (Exception ex) when (
            ex is ArgumentException ||
            ex is InvalidOperationException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id = meetingSessionId });
    }

    [HttpPost("{meetingSessionId:int}/Agenda/{agendaItemId:int}/Start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartAgendaItem(
        int meetingSessionId,
        int agendaItemId,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            meetingSessionId,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.StartAgendaItemAsync(
                meetingSessionId,
                agendaItemId,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "بخش دستور جلسه آغاز شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id = meetingSessionId });
    }

    [HttpPost("{meetingSessionId:int}/Agenda/{agendaItemId:int}/Complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAgendaItem(
        int meetingSessionId,
        int agendaItemId,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            meetingSessionId,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.CompleteAgendaItemAsync(
                meetingSessionId,
                agendaItemId,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "بخش دستور جلسه تکمیل شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id = meetingSessionId });
    }

    [HttpPost("{meetingSessionId:int}/Agenda/{agendaItemId:int}/Skip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SkipAgendaItem(
        int meetingSessionId,
        int agendaItemId,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            meetingSessionId,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.SkipAgendaItemAsync(
                meetingSessionId,
                agendaItemId,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "بخش دستور جلسه رد شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id = meetingSessionId });
    }

    [HttpPost("{meetingSessionId:int}/Agenda/{agendaItemId:int}/Extend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExtendAgendaItem(
        int meetingSessionId,
        int agendaItemId,
        int additionalSeconds,
        CancellationToken cancellationToken)
    {
        var access = await GetMeetingAccessAsync(
            meetingSessionId,
            requireControl: true,
            cancellationToken);

        if (access.Session is null)
            return NotFound();

        if (access.Forbidden)
            return Forbid();

        try
        {
            var result = await _meetingService.ExtendAgendaItemAsync(
                meetingSessionId,
                agendaItemId,
                additionalSeconds,
                CurrentUserId,
                cancellationToken);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "زمان بخش دستور جلسه افزایش یافت.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id = meetingSessionId });
    }
}
