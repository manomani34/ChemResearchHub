
using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Users;

public class EditUserViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(
        200,
        ErrorMessage = "Full name cannot exceed 200 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(
        256,
        ErrorMessage = "Email cannot exceed 256 characters.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please select at least one role.")]
    [MinLength(1, ErrorMessage = "Please select at least one role.")]
    [Display(Name = "Roles")]
    public List<string> RoleNames { get; set; } = new();

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
