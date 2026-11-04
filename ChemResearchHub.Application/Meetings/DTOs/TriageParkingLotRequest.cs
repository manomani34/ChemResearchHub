using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class TriageParkingLotRequest
{
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    public string? AssignedToUserId { get; set; }
}