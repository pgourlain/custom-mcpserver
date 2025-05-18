using Azure.Identity;
using Microsoft.Extensions.Azure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<StorageAccountTools>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddAzureClients(async clientBuilder =>
{
    var connectionString = builder.Configuration["ConnectionStrings:StorageAccount"];
    
    // Set a credential for all clients to use by default
    // DefaultAzureCredential credential = new();
    // clientBuilder.UseCredential(credential);
    clientBuilder.AddTableServiceClient(connectionString);
});
await builder.Build().RunAsync();

