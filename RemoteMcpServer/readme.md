# Ferretly Remote MCP Server

A hosted [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server that exposes read-only tools for the Ferretly External API over HTTP. Each customer connects using their own Ferretly API key — no keys are stored on the server.

Write operations are intentionally not exposed.

## Prerequisites

- .NET 8 SDK (to build/run from source)
- A valid Ferretly External API key (per connecting customer)
- HTTPS in production (API keys transit over the wire)

## Running the server

```bash
dotnet run --project RemoteMcpServer/RemoteMcpServer.csproj
```

By default the server listens on `http://localhost:5000` and `https://localhost:5001`.

## Configuration

`appsettings.json` has one optional setting:

| Key | Description | Default |
|-----|-------------|---------|
| `BaseUrl` | Base URL of the Ferretly External API | `https://api.ferretly.com` |

There is no `ApiKey` setting — each customer supplies their own key at connection time.

## Publishing a self-contained binary

```bash
dotnet publish RemoteMcpServer/RemoteMcpServer.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/remote/
```

Replace `linux-x64` with your target runtime identifier (`win-x64`, `osx-x64`, `osx-arm64`).

## How API key forwarding works

Every inbound MCP request must include the customer's Ferretly API key as an `X-Api-Key` header. The server reads that header from the incoming HTTP request and attaches it to every outgoing call to the Ferretly API. The key is never logged or stored.

If the header is missing or invalid, Ferretly returns a 401 and the tool surfaces that error to the caller.

## Wiring up to Claude Code

Add the server to your Claude Code MCP configuration in `.claude/settings.json` (project-level) or `~/.claude/settings.json` (global):

```json
{
  "mcpServers": {
    "ferretly": {
      "type": "http",
      "url": "https://your-server.com/mcp",
      "headers": {
        "X-Api-Key": "YOUR_FERRETLY_API_KEY"
      }
    }
  }
}
```

Replace `https://your-server.com/mcp` with your deployed server URL and `YOUR_FERRETLY_API_KEY` with the customer's actual key.

To avoid committing the key to source control, use an environment variable reference:

```json
{
  "mcpServers": {
    "ferretly": {
      "type": "http",
      "url": "https://your-server.com/mcp",
      "headers": {
        "X-Api-Key": "${FERRETLY_API_KEY}"
      }
    }
  }
}
```

Then set `FERRETLY_API_KEY` in your shell environment before launching Claude Code.

## Wiring up to Claude.ai

1. Go to **Settings → Integrations** in Claude.ai.
2. Click **Add integration** and select **Custom MCP Server**.
3. Enter your server URL (e.g. `https://your-server.com/mcp`).
4. Add a custom header: **Name** `X-Api-Key`, **Value** your Ferretly API key.
5. Save. The Ferretly tools will appear in your Claude.ai conversations.

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
