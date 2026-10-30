
using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Areas.Admin.Models.Roles;

public class EditRoleViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 2)]
    [Display(Name = "Role Name")]
    public string RoleName { get; set; } = string.Empty;
}
