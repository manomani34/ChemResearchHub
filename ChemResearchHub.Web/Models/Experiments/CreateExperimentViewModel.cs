using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Experiments;

public class CreateExperimentViewModel
{
    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public int BoardId { get; set; }

    [Required(ErrorMessage = "Experiment title is required.")]
    [StringLength(
        300,
        ErrorMessage = "Title cannot exceed 300 characters.")]
    [Display(Name = "Experiment Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(
        5000,
        ErrorMessage = "Description cannot exceed 5000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(
        10000,
        ErrorMessage = "Protocol cannot exceed 10000 characters.")]
    [Display(Name = "Protocol")]
    public string? Protocol { get; set; }

    [Display(Name = "Started At")]
    [DataType(DataType.DateTime)]
    public DateTime? StartedAt { get; set; }

    [Display(Name = "Completed At")]
    [DataType(DataType.DateTime)]
    public DateTime? CompletedAt { get; set; }

    [StringLength(
        10000,
        ErrorMessage = "Notes cannot exceed 10000 characters.")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}