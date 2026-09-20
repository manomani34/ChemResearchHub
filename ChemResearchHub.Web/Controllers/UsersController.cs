using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private static readonly string[] AvailableRoles =
    {
        "Researcher",
        "Reviewer",
        "Admin"
    };

    private readonly IUserService _userService;

    public UsersController(
        IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var users =
            await _userService.GetAllAsync(
                cancellationToken);

        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Roles = AvailableRoles;

        return View(
            new CreateUserViewModel
            {
                RoleName = "Researcher"
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserViewModel model,
        CancellationToken cancellationToken)
    {
        ViewBag.Roles = AvailableRoles;

        if (!AvailableRoles.Contains(
                model.RoleName,
                StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.RoleName),
                "The selected role is not valid.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user =
                await _userService.CreateAsync(
                    model.FullName,
                    model.Email,
                    model.Password,
                    model.RoleName,
                    cancellationToken);

            if (user is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The user could not be created.");

                return View(model);
            }

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        string id,
        CancellationToken cancellationToken)
    {
        var user =
            await _userService.GetByIdAsync(
                id,
                cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        ViewBag.Roles = AvailableRoles;

        return View(
            new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = string.IsNullOrWhiteSpace(
                    user.RoleName)
                    ? "Researcher"
                    : user.RoleName,
                IsActive = user.IsActive
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditUserViewModel model,
        CancellationToken cancellationToken)
    {
        ViewBag.Roles = AvailableRoles;

        if (!AvailableRoles.Contains(
                model.RoleName,
                StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.RoleName),
                "The selected role is not valid.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated =
                await _userService.UpdateAsync(
                    model.Id,
                    model.FullName,
                    model.Email,
                    model.RoleName,
                    model.IsActive,
                    cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }
}