using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Users;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(
        200,
        ErrorMessage = "Full name cannot exceed 200 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(
        ErrorMessage = "Please enter a valid email address.")]
    [StringLength(
        256,
        ErrorMessage = "Email cannot exceed 256 characters.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please select a role.")]
    [Display(Name = "Role")]
    public string RoleName { get; set; } = "Researcher";

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}