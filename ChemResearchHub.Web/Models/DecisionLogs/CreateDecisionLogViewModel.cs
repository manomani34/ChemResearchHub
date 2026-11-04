using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.DecisionLogs;

public class CreateDecisionLogViewModel
{
    [Required]
    public int WorkItemId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int BoardId { get; set; }

    [Required]
    [StringLength(50)]
    public string DecisionType { get; set; } = "Scientific";

    [Required]
    [StringLength(4000, MinimumLength = 2)]
    public string Decision { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Rationale { get; set; }

    [StringLength(4000)]
    public string? Evidence { get; set; }
}