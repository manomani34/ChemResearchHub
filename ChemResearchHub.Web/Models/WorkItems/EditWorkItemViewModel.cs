using System.ComponentModel.DataAnnotations;
using ChemResearchHub.Domain.Enums;

namespace ChemResearchHub.Web.Models.WorkItems;

public class EditWorkItemViewModel
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int BoardId { get; set; }

    public int BoardColumnId { get; set; }

    public string? AssignedToUserId { get; set; }

    [Required(ErrorMessage = "Work item title is required.")]
    [StringLength(
        300,
        ErrorMessage = "Title cannot exceed 300 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(
        5000,
        ErrorMessage = "Description cannot exceed 5000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Type")]
    public WorkItemType Type { get; set; }

    [Range(
        0,
        10,
        ErrorMessage = "Priority must be between 0 and 10.")]
    [Display(Name = "Priority")]
    public int Priority { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; }
}