using System.Text.Json.Serialization;

namespace ChemResearchHub.Application.Bale.Models;

public sealed class BaleChat
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}

