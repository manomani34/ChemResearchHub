using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Web.Models.Meetings;

public class EditMeetingViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "عنوان جلسه الزامی است.")]
    [StringLength(250, MinimumLength = 3)]
    [Display(Name = "عنوان جلسه")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "تاریخ و ساعت جلسه")]
    public DateTime ScheduledStartAt { get; set; }

    [Range(1, 720)]
    [Display(Name = "مدت جلسه (دقیقه)")]
    public int PlannedDurationMinutes { get; set; }
}