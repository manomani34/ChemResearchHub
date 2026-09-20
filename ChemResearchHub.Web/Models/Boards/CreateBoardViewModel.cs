using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Boards;

public class CreateBoardViewModel
{
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Board name is required.")]
    [StringLength(
        200,
        ErrorMessage = "Board name cannot exceed 200 characters.")]
    [Display(Name = "Board Name")]
    public string Name { get; set; } = string.Empty;
}