using System.ComponentModel;
using ModelContextProtocol.Server;
using Newtonsoft.Json;

namespace ExternalApiMcpServer;

[McpServerToolType]
public class FerretlyApiTools
{
    public const string HttpClientName = "FerretlyApi";

    private readonly IHttpClientFactory _httpClientFactory;

    public FerretlyApiTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient(HttpClientName);

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
        using var client = CreateClient();
        var response = await client.GetAsync("api/Subjects");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_subject")]
    [Description("Get a subject by their Ferretly subject ID.")]
    public async Task<string> GetSubjectAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_subject_by_external_id")]
    [Description("Get a subject by their external system ID.")]
    public async Task<string> GetSubjectByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/getByExternalId/{Uri.EscapeDataString(externalId)}");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_screening_status")]
    [Description("Get the background check screening status for a subject by their Ferretly subject ID.")]
    public async Task<string> GetScreeningStatusAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}/screeningstatus");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_screening_status_by_external_id")]
    [Description("Get the background check screening status for a subject by their external system ID.")]
    public async Task<string> GetScreeningStatusByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/{Uri.EscapeDataString(externalId)}/screeningStatusByExternalId");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_background_check_results")]
    [Description("Get the background check results for a subject by their Ferretly subject ID.")]
    public async Task<string> GetBackgroundCheckResultsAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}/backgroundCheckResults");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_background_check_results_by_external_id")]
    [Description("Get the background check results for a subject by their external system ID.")]
    public async Task<string> GetBackgroundCheckResultsByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/getByExternalId/{Uri.EscapeDataString(externalId)}/backgroundCheckResults");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_filtered_posts")]
    [Description("Get the filtered (flagged) social media posts for a subject by their Ferretly subject ID.")]
    public async Task<string> GetFilteredPostsAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}/filteredPosts");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_filtered_posts_by_external_id")]
    [Description("Get the filtered (flagged) social media posts for a subject by their external system ID.")]
    public async Task<string> GetFilteredPostsByExternalIdAsync(
        [Description("The external system ID used by the caller's platform.")] string externalId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/{Uri.EscapeDataString(externalId)}/filteredPostsByExternalId");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "download_background_report")]
    [Description("Download the background report PDF for a subject by their Ferretly subject ID. Returns the report content.")]
    public async Task<string> DownloadBackgroundReportAsync(
        [Description("The Ferretly subject ID (GUID).")] string subjectId)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/Subjects/{Uri.EscapeDataString(subjectId)}/downloadBackgroundReport");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return $"Error {(int)response.StatusCode} {response.ReasonPhrase}: {error}";
        }
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return JsonConvert.SerializeObject(new
        {
            contentType,
            sizeBytes = bytes.Length,
            base64Content = Convert.ToBase64String(bytes)
        });
    }

    [McpServerTool(Name = "list_preference_profiles")]
    [Description("List all preference profiles for the authenticated organization.")]
    public async Task<string> ListPreferenceProfilesAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("api/PreferenceProfiles");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_api_client")]
    [Description("Get API client information for the current API key, including permissions and webhook URL.")]
    public async Task<string> GetApiClientAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("api/ApiClient");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_organization")]
    [Description("Get organization details for the authenticated API client.")]
    public async Task<string> GetOrganizationAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("api/ApiClient/getOrganization");
        return await ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_health_settings")]
    [Description("Get API health status and settings.")]
    public async Task<string> GetHealthSettingsAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("api/Health/settings");
        return await ReadResponseAsync(response);
    }
}
