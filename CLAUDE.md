# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This repo contains two .NET 8 projects that expose the **Ferretly External API** as MCP tools — one for local (stdio) use and one for remote (HTTP) hosting. Both expose the same 13 read-only GET tools.

| Project | Transport | API key source |
|---------|-----------|----------------|
| `LocalMcpServer/` | stdio — runs as a local process | `appsettings.json` / user secrets |
| `RemoteMcpServer/` | HTTP — hosted as a web service | `X-Api-Key` request header from each customer |

## Commands

```bash
# Build entire solution
dotnet build

# Run local stdio server
dotnet run --project LocalMcpServer/LocalMcpServer.csproj

# Run remote HTTP server (default port 5000/5001)
dotnet run --project RemoteMcpServer/RemoteMcpServer.csproj

# Publish as a self-contained single-file binary
dotnet publish LocalMcpServer/LocalMcpServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/local/
dotnet publish RemoteMcpServer/RemoteMcpServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/remote/
# Other runtime IDs: linux-x64, osx-x64, osx-arm64

# Store API key securely for LocalMcpServer local development
dotnet user-secrets set "ApiKey" "your-key" --project LocalMcpServer/LocalMcpServer.csproj
```

There are no test or lint commands — the projects use standard .NET CLI only.

## Architecture

### LocalMcpServer

**`Program.cs`** — Configures a generic `IHost`, redirects logging to stderr (stdout is reserved for the MCP stdio protocol), reads `ApiKey` and `BaseUrl` from config, registers `HttpClient` with the Ferretly key pre-set, then starts an MCP server with stdio transport.

**`FerretlyApiTools.cs`** — 13 `[McpServerTool]`-decorated methods. All use a shared `GetAsync` helper that creates a client from `IHttpClientFactory` and returns the JSON response body (or a base64 JSON envelope for PDF downloads). Tools are auto-discovered from the assembly.

### RemoteMcpServer

**`Program.cs`** — Uses `WebApplication` (ASP.NET Core). Registers `HttpClient` with only `BaseUrl` (no API key). Registers `IHttpContextAccessor`. Starts an MCP server with HTTP transport and maps MCP at the default route via `app.MapMcp()`.

**`FerretlyApiTools.cs`** — Same 13 tools. `IHttpContextAccessor` is injected alongside `IHttpClientFactory`. The private `GetAsync` helper reads `X-Api-Key` from the **incoming** HTTP request headers and adds it to each outgoing Ferretly request — this is how each customer's key is forwarded without server-side storage.

### Key design note

The `X-Api-Key` header flows through: **MCP client → RemoteMcpServer → Ferretly API**. The server never stores or logs it. Customers must pass the header when connecting; if it is missing, Ferretly will return a 401 and the tool will surface that error.

## Configuration

### LocalMcpServer — `appsettings.json`

| Key | Description | Default |
|-----|-------------|---------|
| `ApiKey` | Sent as `X-Api-Key` to Ferretly | *required* |
| `BaseUrl` | Ferretly API base URL | `https://api.ferretly.com` |

User secrets ID: `a3f7c2e1-5b84-4d69-9e23-8f1a0c6b7d45`

### RemoteMcpServer — `appsettings.json`

| Key | Description | Default |
|-----|-------------|---------|
| `BaseUrl` | Ferretly API base URL | `https://api.ferretly.com` |

## Adding New Tools

Add a method to `FerretlyApiTools.cs` in either project (or both), decorated with `[McpServerTool]` and `[Description]`. In `LocalMcpServer`, call `CreateClient()` directly. In `RemoteMcpServer`, use the `GetAsync(url)` helper — it handles the per-request key forwarding automatically. No registration is needed; tools are auto-discovered.
