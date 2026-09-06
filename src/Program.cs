using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<INetStoneService, NetStoneService>();
builder.Services.AddHttpClient<ICommonService, CommonService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36");
});
builder.Services.AddHttpClient<IPaissaHouseService, PaissaHouseService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NetStoneMCP/1.0 (https://github.com/dks50217/NetStoneMCP)");
});
builder.Services.AddHttpClient<IStoreService, StoreService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36");
});
builder.Services.AddHttpClient<IXIVAPIService, XIVAPIService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NetStoneMCP/1.0 (https://github.com/dks50217/NetStoneMCP)");
});
builder.Services.AddHttpClient<ILodeStoneNewsService, LodeStoneNewsService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NetStoneMCP/1.0 (https://github.com/dks50217/NetStoneMCP)");
});
builder.Services.AddHttpClient<IThaliakService, ThaliakService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NetStoneMCP/1.0 (https://github.com/dks50217/NetStoneMCP)");
});
builder.Services.AddScoped<ICustomService, CustomService>();
builder.Services.AddHttpClient<IFFXIVCollectService, FFXIVCollectService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NetStoneMCP/1.0 (https://github.com/dks50217/NetStoneMCP)");
});
builder.Services.AddHttpClient<IHuijiWikiService, HuijiWikiService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NetStoneMCP/1.0 (https://github.com/dks50217/NetStoneMCP)");
});

static async Task InitNetStone(IHost host)
{
    try
    {
        var netStoneService = host.Services.GetRequiredService<INetStoneService>();
        await netStoneService.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Initial NetStoneService initialization failed. Subsequent requests may retry.");
    }
}

var app = builder.Build();

await InitNetStone(app);

if (transportType == "SSE")
{
    app.MapMcp();
}

await app.RunAsync();

