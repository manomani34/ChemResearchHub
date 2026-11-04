using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class UpdateParkingLotRequest
{
    [Required]
    [StringLength(250, MinimumLength = 2)]
    public string Title { get; set; } = null!;

    [StringLength(4000)]
    public string? Summary { get; set; }

    public int? ProjectId { get; set; }
}