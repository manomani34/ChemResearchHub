using ChemResearchHub.Application.ResearchReviews.Dtos;
using ChemResearchHub.Application.ResearchReviews.Interfaces;
using ChemResearchHub.Infrastructure.Identity;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security;
using System.Security.Claims;

namespace ChemResearchHub.Web.Areas.Research.Controllers;

[Area("Research")]
[Authorize]
[Route("Research/Reviews")]
public class ReviewsController : Controller
{
    private readonly IResearchReviewService _reviewService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ReviewsController(
        IResearchReviewService reviewService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _reviewService = reviewService;
        _userManager = userManager;
        _context = context;
    }

    // =========================================================
    // Review List
    // Admin / Researcher / Reviewer
    // =========================================================

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int workItemId,
        CancellationToken cancellationToken)
    {
        if (workItemId <= 0)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!await IsAdminResearcherOrReviewerAsync(userId))
            return Forbid();

        var workItem = await _context.WorkItems
            .AsNoTracking()
            .Where(x => x.Id == workItemId)
            .Select(x => new
            {
                x.Id,
                x.Title
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (workItem is null)
            return NotFound();

        var reviews = await _reviewService.GetByWorkItemIdAsync(
            workItemId,
            cancellationToken);

        var reviewers = await _userManager.GetUsersInRoleAsync("Reviewer");

        var model = new ResearchReviewIndexViewModel
        {
            WorkItemId = workItem.Id,
            WorkItemTitle = workItem.Title,

            Reviews = reviews,

            Reviewers = reviewers
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(x => new ResearchReviewerOptionViewModel
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email
                })
                .ToList()
        };

        return View(model);
    }

    // =========================================================
    // Request Review
    // Admin / Researcher
    // =========================================================

    [HttpPost("request")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestReview(
        int workItemId,
        string? reviewerUserId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!await IsAdminOrResearcherAsync(userId))
            return Forbid();

        if (!string.IsNullOrWhiteSpace(reviewerUserId))
        {
            var reviewers = await _userManager.GetUsersInRoleAsync("Reviewer");

            if (!reviewers.Any(x =>
                    x.Id == reviewerUserId &&
                    x.IsActive))
            {
                TempData["ReviewError"] =
                    "Reviewer انتخاب‌شده معتبر نیست.";

                return RedirectToAction(
                    nameof(Index),
                    new { workItemId });
            }
        }

        var success = await _reviewService.RequestAsync(
            workItemId,
            userId,
            reviewerUserId,
            cancellationToken);

        TempData[success ? "ReviewSuccess" : "ReviewError"] = success
            ? "درخواست Review با موفقیت ثبت شد."
            : "ثبت درخواست Review انجام نشد. احتمالاً یک Review فعال برای این Work Item وجود دارد.";

        return RedirectToAction(
            nameof(Index),
            new { workItemId });
    }

    // =========================================================
    // Start Review
    // Admin / Reviewer
    // =========================================================

    [HttpPost("start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(
        int reviewId,
        int workItemId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!await IsReviewerOrAdminAsync(userId))
            return Forbid();

        var success = await _reviewService.StartAsync(
            reviewId,
            userId,
            cancellationToken);

        TempData[success ? "ReviewSuccess" : "ReviewError"] = success
            ? "Review وارد مرحله بررسی شد."
            : "شروع Review انجام نشد.";

        return RedirectToAction(
            nameof(Index),
            new { workItemId });
    }

    // =========================================================
    // Approve Review
    // Admin / Reviewer
    // =========================================================

    [HttpPost("approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        int reviewId,
        int workItemId,
        string? note,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!await IsReviewerOrAdminAsync(userId))
            return Forbid();

        var currentUser = await _userManager.FindByIdAsync(userId);

        if (currentUser is null)
            return Challenge();

        var isAdmin = await _userManager.IsInRoleAsync(
            currentUser,
            "Admin");

        // IMPORTANT:
        // Approve must call ApproveAsync, not RejectAsync.
        var success = await _reviewService.ApproveAsync(
            reviewId,
            userId,
            note,
            isAdmin,
            cancellationToken);

        TempData[success ? "ReviewSuccess" : "ReviewError"] = success
            ? "Review تایید شد."
            : "تایید Review انجام نشد.";

        return RedirectToAction(
            nameof(Index),
            new { workItemId });
    }

    // =========================================================
    // Reject Review
    // Admin / Reviewer
    // =========================================================

    [HttpPost("reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
        int reviewId,
        int workItemId,
        string? note,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!await IsReviewerOrAdminAsync(userId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(note))
        {
            TempData["ReviewError"] =
                "دلیل رد Review الزامی است.";

            return RedirectToAction(
                nameof(Index),
                new { workItemId });
        }

        var currentUser = await _userManager.FindByIdAsync(userId);

        if (currentUser is null)
            return Challenge();

        var isAdmin = await _userManager.IsInRoleAsync(
            currentUser,
            "Admin");

        var success = await _reviewService.RejectAsync(
            reviewId,
            userId,
            note,
            isAdmin,
            cancellationToken);

        TempData[success ? "ReviewSuccess" : "ReviewError"] = success
            ? "Review رد شد."
            : "رد Review انجام نشد.";

        return RedirectToAction(
            nameof(Index),
            new { workItemId });
    }

    // =========================================================
    // Cancel Review
    // Admin OR Requester
    // =========================================================

    [HttpPost("cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
        int reviewId,
        int workItemId,
        string? note,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var review = await _reviewService.GetByIdAsync(
            reviewId,
            cancellationToken);

        if (review is null)
            return NotFound();

        var currentUser = await _userManager.FindByIdAsync(userId);

        if (currentUser is null)
            return Challenge();

        var isAdmin = await _userManager.IsInRoleAsync(
            currentUser,
            "Admin");

        // Cancel:
        // Admin OR the user who requested the review.
        if (!isAdmin &&
            !string.Equals(
                review.RequestedByUserId,
                userId,
                StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var success = await _reviewService.CancelAsync(
            reviewId,
            userId,
            note,
            cancellationToken);

        TempData[success ? "ReviewSuccess" : "ReviewError"] = success
            ? "درخواست Review لغو شد."
            : "لغو Review انجام نشد.";

        return RedirectToAction(
            nameof(Index),
            new { workItemId });
    }

    // =========================================================
    // Permission Helpers
    // =========================================================

    private async Task<bool> IsAdminOrResearcherAsync(
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
            return false;

        return await _userManager.IsInRoleAsync(user, "Admin")
            || await _userManager.IsInRoleAsync(user, "Researcher");
    }

    private async Task<bool> IsReviewerOrAdminAsync(
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
            return false;

        return await _userManager.IsInRoleAsync(user, "Reviewer")
            || await _userManager.IsInRoleAsync(user, "Admin");
    }

    private async Task<bool> IsAdminResearcherOrReviewerAsync(
        string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
            return false;

        return await _userManager.IsInRoleAsync(user, "Admin")
            || await _userManager.IsInRoleAsync(user, "Researcher")
            || await _userManager.IsInRoleAsync(user, "Reviewer");
    }
}

public class ResearchReviewIndexViewModel
{
    public int WorkItemId { get; init; }

    public string WorkItemTitle { get; init; } = string.Empty;

    public IReadOnlyList<ResearchReviewDto> Reviews { get; init; }
        = Array.Empty<ResearchReviewDto>();

    public IReadOnlyList<ResearchReviewerOptionViewModel> Reviewers { get; init; }
        = Array.Empty<ResearchReviewerOptionViewModel>();
}

public class ResearchReviewerOptionViewModel
{
    public string Id { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string ReferenceCode { get; set; } = string.Empty;
}
