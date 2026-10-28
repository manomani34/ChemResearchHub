
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        IUserService userService,
        RoleManager<IdentityRole> roleManager)
    {
        _userService = userService;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadRolesAsync();

        return View(new CreateUserViewModel
        {
            RoleNames = new List<string> { "Researcher" }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserViewModel model,
        CancellationToken cancellationToken)
    {
        await LoadRolesAsync();

        await ValidateSelectedRolesAsync(
            model.RoleNames,
            nameof(model.RoleNames));

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userService.CreateAsync(
                model.FullName,
                model.Email,
                model.Password,
                model.RoleNames,
                cancellationToken);

            if (user is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The user could not be created.");

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        string id,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(
            id,
            cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        await LoadRolesAsync();

        return View(new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleNames = user.RoleNames.ToList(),
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditUserViewModel model,
        CancellationToken cancellationToken)
    {
        await LoadRolesAsync();

        await ValidateSelectedRolesAsync(
            model.RoleNames,
            nameof(model.RoleNames));

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated = await _userService.UpdateAsync(
                model.Id,
                model.FullName,
                model.Email,
                model.RoleNames,
                model.IsActive,
                cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private async Task LoadRolesAsync()
    {
        ViewBag.Roles = _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => x.Name!)
            .ToList();
    }

    private async Task ValidateSelectedRolesAsync(
        IEnumerable<string>? selectedRoles,
        string modelStateKey)
    {
        var roles = (selectedRoles ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roles.Count == 0)
        {
            ModelState.AddModelError(
                modelStateKey,
                "Please select at least one role.");

            return;
        }

        var availableRoles = _roleManager.Roles
            .Select(x => x.Name!)
            .ToList();

        foreach (var role in roles)
        {
            if (!availableRoles.Contains(
                    role,
                    StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    modelStateKey,
                    $"The selected role '{role}' is not valid.");
            }
        }
    }
}


