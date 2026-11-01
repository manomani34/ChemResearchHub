using System.Text.Json.Serialization;

namespace ChemResearchHub.Application.Bale.Models;

public sealed class BaleMessage
{
    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }

    [JsonPropertyName("date")]
    public long Date { get; set; }

    [JsonPropertyName("chat")]
    public BaleChat? Chat { get; set; }

    [JsonPropertyName("from")]
    public BaleUser? From { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

