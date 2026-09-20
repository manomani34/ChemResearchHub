using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Projects;

public class EditProjectViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(
        200,
        ErrorMessage = "Project name cannot exceed 200 characters.")]
    [Display(Name = "Project Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(
        2000,
        ErrorMessage = "Description cannot exceed 2000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}