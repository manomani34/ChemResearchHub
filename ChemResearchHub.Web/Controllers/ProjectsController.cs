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
        return View();
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

        await _projectService.CreateAsync(
            model.Name,
            model.Description,
            cancellationToken);

        return RedirectToAction(nameof(Index));
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

        var model = new EditProjectViewModel
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

        var updated =
            await _projectService.UpdateAsync(
                model.Id,
                model.Name,
                model.Description,
                cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        await _projectService.SetActiveAsync(
            model.Id,
            model.IsActive,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}