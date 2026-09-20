using ChemResearchHub.Application.Projects.Interfaces;
using ChemResearchHub.Web.Models.Projects;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;

    public ProjectsController(
        IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var projects =
            await _projectService.GetAllAsync(
                cancellationToken);

        return View(projects);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(
            new CreateProjectViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateProjectViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _projectService.CreateAsync(
                model.Name.Trim(),
                model.Description?.Trim(),
                cancellationToken);

            TempData["SuccessMessage"] =
                "پروژه پژوهشی با موفقیت ایجاد شد.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
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
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var project =
            await _projectService.GetByIdAsync(
                id,
                cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        var model =
            new ProjectDetailsViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsActive = project.IsActive
            };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var project =
            await _projectService.GetByIdAsync(
                id,
                cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        var model =
            new EditProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsActive = project.IsActive
            };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditProjectViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated =
                await _projectService.UpdateAsync(
                    model.Id,
                    model.Name.Trim(),
                    model.Description?.Trim(),
                    cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            await _projectService.SetActiveAsync(
                model.Id,
                model.IsActive,
                cancellationToken);

            TempData["SuccessMessage"] =
                "اطلاعات پروژه با موفقیت به‌روزرسانی شد.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
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