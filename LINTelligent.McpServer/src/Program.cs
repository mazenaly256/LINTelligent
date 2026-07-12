using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);  // To initialize the DI container

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;       // prevents exporting logs via stdout stream/channel, uses stderr channel instead, keeping stdout clean for communication with AI agent
});

builder.Services.AddHttpClient("LINTelligent Service Api", client =>
{
    client.BaseAddress = new Uri("https://lintelligent-production.up.railway.app");
});   // To register IHttpClientFactory in DI container, with a pre-defined base URL

builder.Services.AddMcpServer()     // To register the required dependencies to make the app act as MCP server
    .WithStdioServerTransport()     // To communicate with stdin and stdout
    .WithToolsFromAssembly();   // To indicate where the tools (available methods) to call are, those that decorated with [McpServerTool] attribute

var app = builder.Build();

await app.RunAsync();   // To block the execution and keep the application process running
