using ExternalApiMcpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// MCP stdio transport uses stdout for protocol messages. Redirect all logging
// to stderr so it does not corrupt the MCP message stream.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

var apiKey = builder.Configuration["ApiKey"] ?? "";
var baseUrl = builder.Configuration["BaseUrl"] ?? "https://api.ferretly.com";

builder.Services.AddHttpClient(FerretlyApiTools.HttpClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
