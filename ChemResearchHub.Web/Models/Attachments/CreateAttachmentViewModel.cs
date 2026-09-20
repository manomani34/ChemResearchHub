using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ChemResearchHub.Web.Models.Attachments;

public class CreateAttachmentViewModel
{
    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public int BoardId { get; set; }

    [Required(ErrorMessage = "Please select a file.")]
    [Display(Name = "File")]
    public IFormFile? File { get; set; }

    [StringLength(
        2000,
        ErrorMessage = "Description cannot exceed 2000 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
}