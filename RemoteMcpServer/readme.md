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

## Deploying to Azure App Service

### 1. Create the App Service

```bash
# Set these for your environment
RESOURCE_GROUP=my-resource-group
APP_NAME=my-ferretly-mcp-server
APP_PLAN=my-ferretly-mcp-plan
LOCATION=eastus

# Create resource group (skip if it already exists)
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service plan (Linux, free tier — scale up as needed)
az appservice plan create \
  --name $APP_PLAN \
  --resource-group $RESOURCE_GROUP \
  --is-linux \
  --sku F1

# Create the web app with .NET 8 runtime
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_PLAN \
  --runtime "DOTNETCORE:8.0"
```

### 2. Retrieve the server URL

```bash
az webapp show \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query defaultHostName \
  --output tsv
```

The MCP endpoint will be `https://<defaultHostName>` (the root path — `app.MapMcp()` with no argument maps to `/`).

### 3. Retrieve the publish profile

```bash
az webapp deployment list-publishing-profiles \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --xml
```

Copy the entire XML output and save it as the `AZURE_WEBAPP_PUBLISH_PROFILE` secret in your GitHub repo.

### 4. Add GitHub secrets

Add these two secrets under **Settings → Secrets and variables → Actions** in your GitHub repo:

| Secret | Value |
|--------|-------|
| `AZURE_WEBAPP_NAME` | The value you used for `$APP_NAME` above |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | The XML output from step 3 |

The GitHub Actions workflow (`.github/workflows/deploy-remote-mcp-server.yml`) will deploy automatically on every push to `main` that touches `RemoteMcpServer/`, or you can trigger it manually from the GitHub Actions UI.

The workflow publishes a framework-dependent `linux-x64` build — the `.NET 8` runtime is provided by the App Service plan created above.

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
      "url": "https://your-server.azurewebsites.net",
      "headers": {
        "X-Api-Key": "YOUR_FERRETLY_API_KEY"
      }
    }
  }
}
```

Replace `https://your-server.azurewebsites.net` with your deployed server URL and `YOUR_FERRETLY_API_KEY` with the customer's actual key.

To avoid committing the key to source control, use an environment variable reference:

```json
{
  "mcpServers": {
    "ferretly": {
      "type": "http",
      "url": "https://your-server.azurewebsites.net",
      "headers": {
        "X-Api-Key": "${FERRETLY_API_KEY}"
      }
    }
  }
}
```

Then set `FERRETLY_API_KEY` in your shell environment before launching Claude Code.

## Wiring up to Claude Desktop

Claude Desktop does not support the native `url`/`headers` MCP config format — it requires a local process as a bridge. The `mcp-remote` npm package handles this.

### Prerequisites

- [Node.js](https://nodejs.org/) (LTS recommended)
- `mcp-remote` installed globally:

```bash
npm install -g mcp-remote
```

> **Windows note:** Do not use `npx mcp-remote` in the Claude Desktop config. If Node.js is installed in `C:\Program Files\nodejs` (the default), `npx` resolves to a path with a space that Claude Desktop passes unquoted to `cmd.exe`, causing a "not recognized" error. Installing `mcp-remote` globally places it in `%APPDATA%\npm\` (no spaces) and avoids the problem.

### Configuration

Add the following to `claude_desktop_config.json` (open it from Claude Desktop → Settings → Developer):

```json
{
  "mcpServers": {
    "ferretly-remote": {
      "command": "mcp-remote",
      "args": [
        "https://your-server.azurewebsites.net",
        "--header",
        "X-Api-Key: YOUR_FERRETLY_API_KEY"
      ]
    }
  }
}
```

Replace `https://your-server.azurewebsites.net` with your deployed server URL and `YOUR_FERRETLY_API_KEY` with the customer's actual key. Restart Claude Desktop after saving.

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
