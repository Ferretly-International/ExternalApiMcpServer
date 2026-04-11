# ExternalApiMcpServer

A [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server that exposes read-only tools for the Ferretly External API. AI assistants (Claude, GitHub Copilot, etc.) can use these tools to query subject data, background check results, and screening status directly.

Write operations — creating subjects, initiating background checks, updating webhooks, or refreshing API keys — are intentionally not exposed.

## Prerequisites

- .NET 8 SDK
- A valid Ferretly External API key

## Configuration

Edit `appsettings.json` before running:

```json
{
  "ApiKey": "YOUR_API_KEY_HERE",
  "BaseUrl": "https://api.ferretly.com"
}
```

| Key | Description | Default |
|-----|-------------|---------|
| `ApiKey` | Ferretly External API key (sent as `X-Api-Key` header) | *(required)* |
| `BaseUrl` | Base URL of the Ferretly External API | `https://api.ferretly.com` |

> **Note:** Do not commit a real API key to source control. Use the .NET user secrets store for local development:
> ```
> dotnet user-secrets set "ApiKey" "your-real-key"
> ```

## Running the server

```bash
dotnet run
```

The server communicates over stdio (standard input/output), which is the MCP standard transport. It does not open a network port.

## Publishing a self-contained binary

To produce a single executable that does not require the .NET runtime to be installed:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/
```

Replace `win-x64` with the target runtime identifier for your platform (e.g. `linux-x64`, `osx-x64`, `osx-arm64`).

The output directory will contain `ExternalApiMcpServer.exe` (Windows) or `ExternalApiMcpServer` (Linux/macOS) alongside a copy of `appsettings.json`. Edit `appsettings.json` in the output directory with your API key before distributing or running.

## Wiring up to Claude Code

Add the server to your Claude Code MCP configuration. In `.claude/settings.json` (or the workspace settings), add:

```json
{
  "mcpServers": {
    "ferretly": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/DataAPI/ExternalApiMcpServer/ExternalApiMcpServer.csproj"]
    }
  }
}
```

Or point to the published binary:

```json
{
  "mcpServers": {
    "ferretly": {
      "command": "path/to/ExternalApiMcpServer.exe"
    }
  }
}
```

## Available tools

| Tool | Description |
|------|-------------|
| `list_subjects` | List all subjects for the authenticated organization |
| `get_subject` | Get a subject by their Ferretly subject ID |
| `get_subject_by_external_id` | Get a subject by their external system ID |
| `get_screening_status` | Get background check status by Ferretly subject ID |
| `get_screening_status_by_external_id` | Get background check status by external system ID |
| `get_background_check_results` | Get full background check results by Ferretly subject ID |
| `get_background_check_results_by_external_id` | Get background check results by external system ID |
| `get_filtered_posts` | Get flagged social media posts by Ferretly subject ID |
| `get_filtered_posts_by_external_id` | Get flagged social media posts by external system ID |
| `download_background_report` | Download the background report PDF (returned as base64) |
| `list_preference_profiles` | List all preference profiles for the organization |
| `get_api_client` | Get API client info for the current API key |
| `get_organization` | Get organization details for the authenticated API client |
| `get_health_settings` | Get API health status and settings |
