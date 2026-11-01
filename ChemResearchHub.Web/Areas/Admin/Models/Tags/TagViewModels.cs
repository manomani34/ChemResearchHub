using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Areas.Admin.Models.Tags;

public class TagFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Tag Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Enter a valid HEX color, e.g. #2563EB.")]
    [Display(Name = "Color")]
    public string Color { get; set; } = "#64748B";
}

public class TagListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#64748B";
    public bool IsActive { get; set; }
}
