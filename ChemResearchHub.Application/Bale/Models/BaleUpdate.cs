using System.Text.Json.Serialization;

namespace ChemResearchHub.Application.Bale.Models;

public sealed class BaleUpdate
{
    [JsonPropertyName("update_id")]
    public long UpdateId { get; set; }

    [JsonPropertyName("message")]
    public BaleMessage? Message { get; set; }
}

