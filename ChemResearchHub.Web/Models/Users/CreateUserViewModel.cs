using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Users;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(
        200,
        ErrorMessage = "Full name cannot exceed 200 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(
        256,
        ErrorMessage = "Email cannot exceed 256 characters.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Password must be between 6 and 100 characters.")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role.")]
    [Display(Name = "Role")]
    public string RoleName { get; set; } = "Researcher";
}