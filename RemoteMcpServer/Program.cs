using RemoteMcpServer;

var builder = WebApplication.CreateBuilder(args);

var baseUrl = builder.Configuration["BaseUrl"] ?? "https://api.ferretly.com";

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient(FerretlyApiTools.HttpClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    // API key is not set here — it is read from the incoming request and
    // forwarded to Ferretly per-call so each customer uses their own key.
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

await app.RunAsync();
