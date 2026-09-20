using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Samples;

public class CreateSampleViewModel
{
    public int ExperimentId { get; set; }

    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public int BoardId { get; set; }

    [Required(ErrorMessage = "Sample code is required.")]
    [StringLength(
        100,
        ErrorMessage = "Sample code cannot exceed 100 characters.")]
    [Display(Name = "Sample Code")]
    public string SampleCode { get; set; } = string.Empty;

    [StringLength(
        300,
        ErrorMessage = "Name cannot exceed 300 characters.")]
    [Display(Name = "Sample Name")]
    public string? Name { get; set; }

    [StringLength(
        200,
        ErrorMessage = "Sample type cannot exceed 200 characters.")]
    [Display(Name = "Sample Type")]
    public string? SampleType { get; set; }

    [StringLength(
        300,
        ErrorMessage = "Matrix cannot exceed 300 characters.")]
    [Display(Name = "Matrix")]
    public string? Matrix { get; set; }

    [StringLength(
        5000,
        ErrorMessage = "Preparation method cannot exceed 5000 characters.")]
    [Display(Name = "Preparation Method")]
    public string? PreparationMethod { get; set; }

    [Display(Name = "Collected At")]
    [DataType(DataType.DateTime)]
    public DateTime? CollectedAt { get; set; }

    [StringLength(
        300,
        ErrorMessage = "External reference cannot exceed 300 characters.")]
    [Display(Name = "External Reference")]
    public string? ExternalReference { get; set; }

    [StringLength(
        5000,
        ErrorMessage = "Description cannot exceed 5000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(
        10000,
        ErrorMessage = "Notes cannot exceed 10000 characters.")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}