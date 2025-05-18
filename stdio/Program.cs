using Azure.Identity;
using Microsoft.Extensions.Azure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddAzureClients(clientBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("StorageAccount");
    // Set a credential for all clients to use by default
    // DefaultAzureCredential credential = new();
    // clientBuilder.UseCredential(credential);
    clientBuilder.AddTableServiceClient(connectionString);
});
await builder.Build().RunAsync();

