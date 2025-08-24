using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Information;
});

var transportType = builder.Configuration["TransportType"];

var mcpBuilder = builder.Services
    .AddMcpServer();

if (transportType == "SSE")
{
    mcpBuilder.WithHttpTransport();
}
else
{
    mcpBuilder.WithStdioServerTransport();
}

mcpBuilder.WithToolsFromAssembly();

builder.Services.AddSingleton<INetStoneService, NetStoneService>();
builder.Services.AddHttpClient<ICommonService, CommonService>();
builder.Services.AddHttpClient<IPaissaHouseService, PaissaHouseService>();
builder.Services.AddHttpClient<IStoreService, StoreService>();
builder.Services.AddHttpClient<IXIVAPIService, XIVAPIService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ILodeStoneNewsService, LodeStoneNewsService>();

static async Task InitNetStone(IHost host)
{
    var netStoneService = host.Services.GetRequiredService<INetStoneService>();
    await netStoneService.InitializeAsync();
}

var app = builder.Build();

await InitNetStone(app);

if (transportType == "SSE")
{
    app.MapMcp();
}

await app.RunAsync();

