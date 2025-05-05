using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetStoneMCP.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddMemoryCache();

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddSingleton<INetStoneService, NetStoneService>();

static async Task InitNetStone(IHost host)
{
    var netStoneService = host.Services.GetRequiredService<INetStoneService>();
    await netStoneService.InitializeAsync();
}

var app = builder.Build();

await InitNetStone(app);

await app.RunAsync();

