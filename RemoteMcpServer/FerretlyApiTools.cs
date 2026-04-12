using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;
using Shared.Models;

namespace RemoteMcpServer;

[McpServerToolType]
public class FerretlyApiTools
{
    public const string HttpClientName = "FerretlyApi";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FerretlyApiTools(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<string> GetAsync(string url)
    {
        using var client = _httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var apiKey = _httpContextAccessor.HttpContext?.Request.Headers["X-Api-Key"].ToString();
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Add("X-Api-Key", apiKey);
        var response = await client.SendAsync(request);
        return await ReadResponseAsync(response);
    }

    private static async Task<string> ReadResponseAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return $"Error {(int)response.StatusCode} {response.ReasonPhrase}: {body}";
        return body;
    }

    [McpServerTool(Name = "list_subjects")]
    [Description("List all subjects for the authenticated organization.")]
    public async Task<string> ListSubjectsAsync()
    {
        var body = await GetAsync("api/Subjects");
        if (body.StartsWith("Error "))
            return body;
        var subjects = JsonSerializer.Deserialize<List<SubjectSummary>>(body);
        return JsonSerializer.Serialize(subjects);
    }

    [McpServerTool(Name = "get_subject")]
    [Description("Get a subject by their Ferretly subject ID.")]
    public Task<string> GetSubjectAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId) =>
        GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}");

    [McpServerTool(Name = "get_subject_by_external_id")]
    [Description("Get a subject by their external system ID.")]
    public Task<string> GetSubjectByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId) =>
        GetAsync($"api/Subjects/getByExternalId/{Uri.EscapeDataString(externalId)}");

    [McpServerTool(Name = "get_screening_status")]
    [Description("Get the background check screening status for a subject by their Ferretly subject ID.")]
    public Task<string> GetScreeningStatusAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId) =>
        GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}/screeningstatus");

    [McpServerTool(Name = "get_screening_status_by_external_id")]
    [Description("Get the background check screening status for a subject by their external system ID.")]
    public Task<string> GetScreeningStatusByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId) =>
        GetAsync($"api/Subjects/{Uri.EscapeDataString(externalId)}/screeningStatusByExternalId");

    [McpServerTool(Name = "get_background_check_results")]
    [Description("Get the background check results for a subject by their Ferretly subject ID.")]
    public Task<string> GetBackgroundCheckResultsAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId) =>
        GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}/backgroundCheckResults");

    [McpServerTool(Name = "get_background_check_results_by_external_id")]
    [Description("Get the background check results for a subject by their external system ID.")]
    public Task<string> GetBackgroundCheckResultsByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId) =>
        GetAsync($"api/Subjects/getByExternalId/{Uri.EscapeDataString(externalId)}/backgroundCheckResults");

    [McpServerTool(Name = "get_filtered_posts")]
    [Description("Get the filtered (flagged) social media posts for a subject by their Ferretly subject ID.")]
    public Task<string> GetFilteredPostsAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId) =>
        GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}/filteredPosts");

    [McpServerTool(Name = "get_filtered_posts_by_external_id")]
    [Description("Get the filtered (flagged) social media posts for a subject by their external system ID.")]
    public Task<string> GetFilteredPostsByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId) =>
        GetAsync($"api/Subjects/{Uri.EscapeDataString(externalId)}/filteredPostsByExternalId");

    [McpServerTool(Name = "download_background_report")]
    [Description("Download the background report PDF for a subject by their Ferretly subject ID. Returns the report content as base64.")]
    public async Task<string> DownloadBackgroundReportAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId)
    {
        using var client = _httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"api/Subjects/{Uri.EscapeDataString(subjectId)}/downloadBackgroundReport");
        var apiKey = _httpContextAccessor.HttpContext?.Request.Headers["X-Api-Key"].ToString();
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Add("X-Api-Key", apiKey);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return $"Error {(int)response.StatusCode} {response.ReasonPhrase}: {error}";
        }
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return JsonSerializer.Serialize(new
        {
            contentType,
            sizeBytes = bytes.Length,
            base64Content = Convert.ToBase64String(bytes)
        });
    }

    [McpServerTool(Name = "list_preference_profiles")]
    [Description("List all preference profiles for the authenticated organization.")]
    public Task<string> ListPreferenceProfilesAsync() =>
        GetAsync("api/PreferenceProfiles");

    [McpServerTool(Name = "get_api_client")]
    [Description("Get API client information for the current API key, including permissions and webhook URL.")]
    public Task<string> GetApiClientAsync() =>
        GetAsync("api/ApiClient");

    [McpServerTool(Name = "get_organization")]
    [Description("Get organization details for the authenticated API client.")]
    public Task<string> GetOrganizationAsync() =>
        GetAsync("api/ApiClient/getOrganization");

    [McpServerTool(Name = "get_health_settings")]
    [Description("Get API health status and settings.")]
    public Task<string> GetHealthSettingsAsync() =>
        GetAsync("api/Health/settings");
}
