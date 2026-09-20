using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Results;

public class EditResultViewModel
{
    public int Id { get; set; }

    public int SampleId { get; set; }

    public int ExperimentId { get; set; }

    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public int BoardId { get; set; }

    [Required(ErrorMessage = "Metric name is required.")]
    [StringLength(
        300,
        ErrorMessage = "Metric name cannot exceed 300 characters.")]
    [Display(Name = "Metric Name")]
    public string MetricName { get; set; } = string.Empty;

    [Display(Name = "Numeric Value")]
    public decimal? NumericValue { get; set; }

    [StringLength(
        5000,
        ErrorMessage = "Text value cannot exceed 5000 characters.")]
    [Display(Name = "Text Value")]
    public string? TextValue { get; set; }

    [StringLength(
        100,
        ErrorMessage = "Unit cannot exceed 100 characters.")]
    [Display(Name = "Unit")]
    public string? Unit { get; set; }

    [StringLength(
        1000,
        ErrorMessage = "Method cannot exceed 1000 characters.")]
    [Display(Name = "Method")]
    public string? Method { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Pending";

    [StringLength(
        10000,
        ErrorMessage = "Evidence cannot exceed 10000 characters.")]
    [Display(Name = "Evidence")]
    public string? Evidence { get; set; }

    [StringLength(
        10000,
        ErrorMessage = "Notes cannot exceed 10000 characters.")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}