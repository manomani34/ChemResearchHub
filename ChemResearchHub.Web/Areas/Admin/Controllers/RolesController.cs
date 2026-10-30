
using ChemResearchHub.Infrastructure.Identity;
using ChemResearchHub.Web.Areas.Admin.Models.Roles;
using ChemResearchHub.Web.Authorization.Permissions;
using ChemResearchHub.Web.Authorization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRolePermissionService _rolePermissionService;

    public RolesController(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IRolePermissionService rolePermissionService)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var model = new List<RoleListItemViewModel>();

        foreach (var role in roles)
        {
            var users = await _userManager.GetUsersInRoleAsync(
                role.Name!);

            model.Add(new RoleListItemViewModel
            {
                Id = role.Id,
                Name = role.Name!,
                UserCount = users.Count,
                IsSystemRole = SystemRoles.IsSystemRole(role.Name)
            });
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateRoleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateRoleViewModel model)
    {
        model.RoleName = model.RoleName?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
            return View(model);

        if (await _roleManager.RoleExistsAsync(model.RoleName))
        {
            ModelState.AddModelError(
                nameof(model.RoleName),
                "A role with this name already exists.");

            return View(model);
        }

        var result = await _roleManager.CreateAsync(
            new IdentityRole(model.RoleName));

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        TempData["SuccessMessage"] = "Role created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();

        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            return NotFound();

        if (SystemRoles.IsSystemRole(role.Name))
        {
            TempData["ErrorMessage"] =
                "System roles cannot be renamed.";

            return RedirectToAction(nameof(Index));
        }

        return View(new EditRoleViewModel
        {
            Id = role.Id,
            RoleName = role.Name!
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditRoleViewModel model)
    {
        model.RoleName = model.RoleName?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
            return View(model);

        var role = await _roleManager.FindByIdAsync(model.Id);

        if (role is null)
            return NotFound();

        if (SystemRoles.IsSystemRole(role.Name))
        {
            TempData["ErrorMessage"] =
                "System roles cannot be renamed.";

            return RedirectToAction(nameof(Index));
        }

        if (await _roleManager.RoleExistsAsync(model.RoleName)
            && !string.Equals(
                role.Name,
                model.RoleName,
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.RoleName),
                "A role with this name already exists.");

            return View(model);
        }

        role.Name = model.RoleName;

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        TempData["SuccessMessage"] = "Role updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();

        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            return NotFound();

        if (SystemRoles.IsSystemRole(role.Name))
        {
            TempData["ErrorMessage"] =
                "System roles cannot be deleted.";

            return RedirectToAction(nameof(Index));
        }

        var users = await _userManager.GetUsersInRoleAsync(
            role.Name!);

        if (users.Count > 0)
        {
            TempData["ErrorMessage"] =
                $"This role cannot be deleted because {users.Count} user(s) are assigned to it.";

            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] =
                string.Join(" ", result.Errors.Select(x => x.Description));

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Role deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Permissions(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();

        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            return NotFound();

        var assignedPermissions =
            await _rolePermissionService.GetPermissionsAsync(role.Id);

        var assignedSet = assignedPermissions.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        var groups = PermissionCatalog.All
            .GroupBy(x => x.Group)
            .Select(group => new RolePermissionGroupViewModel
            {
                Name = group.Key,
                Permissions = group.Select(permission =>
                    new RolePermissionItemViewModel
                    {
                        Name = permission.Name,
                        DisplayName = permission.DisplayName,
                        Description = permission.Description,
                        IsSelected = assignedSet.Contains(permission.Name)
                    }).ToList()
            })
            .ToList();

        var model = new ManageRolePermissionsViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name!,
            Groups = groups,
            SelectedPermissions = assignedPermissions.ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(
        ManageRolePermissionsViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.RoleId))
            return BadRequest();

        var role = await _roleManager.FindByIdAsync(model.RoleId);

        if (role is null)
            return NotFound();

        var result = await _rolePermissionService.SetPermissionsAsync(
            role.Id,
            model.SelectedPermissions ?? []);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(
                " ",
                result.Errors.Select(x => x.Description));

            return RedirectToAction(nameof(Permissions), new { id = role.Id });
        }

        TempData["SuccessMessage"] =
            $"Permissions for role '{role.Name}' updated successfully.";

        return RedirectToAction(nameof(Permissions), new { id = role.Id });
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }
    }
}

public class RoleListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public bool IsSystemRole { get; set; }
}
