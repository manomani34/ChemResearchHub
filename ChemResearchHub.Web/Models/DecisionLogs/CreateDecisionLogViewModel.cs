using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.DecisionLogs;

public class CreateDecisionLogViewModel
{
    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public int BoardId { get; set; }

    [Required(ErrorMessage = "Please select a decision type.")]
    [Display(Name = "Decision Type")]
    public string DecisionType { get; set; } = "Scientific";

    [Required(ErrorMessage = "Decision is required.")]
    [StringLength(
        5000,
        ErrorMessage = "Decision cannot exceed 5000 characters.")]
    [Display(Name = "Decision")]
    public string Decision { get; set; } = string.Empty;

    [StringLength(
        10000,
        ErrorMessage = "Rationale cannot exceed 10000 characters.")]
    [Display(Name = "Rationale")]
    public string? Rationale { get; set; }

    [StringLength(
        10000,
        ErrorMessage = "Evidence cannot exceed 10000 characters.")]
    [Display(Name = "Evidence")]
    public string? Evidence { get; set; }
}