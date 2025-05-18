using Azure.Identity;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<StorageAccountTools>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddAzureClients(async clientBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("StorageAccount");
    
    // Set a credential for all clients to use by default
    // DefaultAzureCredential credential = new();
    // clientBuilder.UseCredential(credential);
    clientBuilder.AddTableServiceClient(connectionString);
});

var app = builder.Build();

app.MapMcp();

app.Run();