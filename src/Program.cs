using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Information;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton<INetStoneService, NetStoneService>();
builder.Services.AddHttpClient<ICommonService, CommonService>();
builder.Services.AddHttpClient<IPaissaHouseService, PaissaHouseService>();
builder.Services.AddHttpClient<IStoreService, StoreService>();

static async Task InitNetStone(IHost host)
{
    var netStoneService = host.Services.GetRequiredService<INetStoneService>();
    await netStoneService.InitializeAsync();
}

var app = builder.Build();

await InitNetStone(app);

await app.RunAsync();

