using System.Text.Json.Serialization;

namespace Shared.Models;

public class SubjectSummary
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("externalSystemId")]
    public string? ExternalSystemId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("score")]
    public int? Score { get; init; }

    [JsonPropertyName("flaggedPosts")]
    public int FlaggedPosts { get; init; }

    [JsonPropertyName("totalPosts")]
    public int TotalPosts { get; init; }

    [JsonPropertyName("lastRunDate")]
    public DateTimeOffset? LastRunDate { get; init; }

    [JsonPropertyName("lastRunStatus")]
    public string? LastRunStatus { get; init; }

    [JsonPropertyName("isBackgroundCheckInProgress")]
    public bool IsBackgroundCheckInProgress { get; init; }

    [JsonPropertyName("isEnabledForContinuousScreening")]
    public bool IsEnabledForContinuousScreening { get; init; }

    [JsonPropertyName("dateAdded")]
    public DateTimeOffset? DateAdded { get; init; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; init; }

    [JsonPropertyName("riskMakeups")]
    public RiskMakeup[]? RiskMakeups { get; init; }
}
