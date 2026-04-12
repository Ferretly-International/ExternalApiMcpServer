using System.Text.Json.Serialization;

namespace Shared.Models;

public class RiskMakeup
{
    [JsonPropertyName("riskName")]
    public string? RiskName { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }
}
