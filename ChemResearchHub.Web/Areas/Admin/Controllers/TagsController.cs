using ChemResearchHub.Application.Tags.Dtos;
using ChemResearchHub.Application.Tags.Interfaces;
using ChemResearchHub.Web.Areas.Admin.Models.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TagsController : Controller
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tags = await _tagService.GetAllAsync(
            cancellationToken: cancellationToken);

        var model = tags.Select(x => new TagListItemViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Color = x.Color,
            IsActive = x.IsActive
        }).ToList();

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TagFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TagFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Color = model.Color?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _tagService.CreateAsync(new CreateTagDto
            {
                Name = model.Name,
                Description = model.Description?.Trim(),
                Color = model.Color
            }, cancellationToken);

            TempData["SuccessMessage"] = "Tag created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (
            ex is ArgumentException ||
            ex is InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.GetByIdAsync(id, cancellationToken);

        if (tag is null)
            return NotFound();

        return View(new TagFormViewModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            Color = tag.Color
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        TagFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Color = model.Color?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var updated = await _tagService.UpdateAsync(
                model.Id,
                new CreateTagDto
                {
                    Name = model.Name,
                    Description = model.Description?.Trim(),
                    Color = model.Color
                },
                cancellationToken);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] = "Tag updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (
            ex is ArgumentException ||
            ex is InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(
        int id,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.GetByIdAsync(id, cancellationToken);

        if (tag is null)
            return NotFound();

        var success = tag.IsActive
            ? await _tagService.DeactivateAsync(id, cancellationToken)
            : await _tagService.ActivateAsync(id, cancellationToken);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? $"Tag '{tag.Name}' {(tag.IsActive ? "deactivated" : "activated")} successfully."
            : "The Tag could not be updated.";

        return RedirectToAction(nameof(Index));
    }
}
