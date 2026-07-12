using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);  // To initialize the DI container

builder.Services.AddHttpClient();   // To register IHttpClientFactory in DI container

builder.Services.AddMcpServer()     // To register the required dependencies to make the app act as MCP server
    .WithStdioServerTransport()     // To communicate with stdin and stdout
    .WithToolsFromAssembly();   // To indicate where the tools (available methods) to call are, those that decorated with [McpServerTool] attribute

var app = builder.Build();

await app.RunAsync();   // To block the execution and keep the application process running
