# Ferretly External API — MCP Servers

[Model Context Protocol (MCP)](https://modelcontextprotocol.io) servers that expose read-only tools for the [Ferretly External API](https://api.ferretly.com). AI assistants (Claude, GitHub Copilot, etc.) can use these tools to query subject data, background check results, and screening status directly.

Two deployment options are provided depending on your use case:

| Project | Transport | Best for |
|---------|-----------|----------|
| [`LocalMcpServer/`](LocalMcpServer/readme.md) | stdio | Personal use — runs as a local process on the developer's machine |
| [`RemoteMcpServer/`](RemoteMcpServer/readme.md) | HTTP | Customer-facing — hosted as a shared web service; each customer supplies their own API key |

## Available tools

Both servers expose the same 13 read-only tools:

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

## Code structure

`FerretlyApiTools.cs` exists in both server projects and looks like duplication, but the two copies differ in a load-bearing way:

- **`LocalMcpServer`** configures its `HttpClient` with the API key at startup — the key is fixed for the lifetime of the process.
- **`RemoteMcpServer`** has no key at startup. Its `GetAsync` helper reads `X-Api-Key` from the *incoming* HTTP request on every call (via `IHttpContextAccessor`) and forwards it to Ferretly — this is what makes per-customer isolation work.

Consolidating into a shared base class would require adding the MCP framework as a dependency of the `Shared` library and abstracting the HTTP execution strategy — meaningful complexity for 13 one-liner methods. The duplication is intentional at this scale.

`Shared/` holds models that genuinely have no framework dependency and are needed by both projects (`SubjectSummary`, `RiskMakeup`).

## Prerequisites

- .NET 8 SDK
- A valid Ferretly External API key
