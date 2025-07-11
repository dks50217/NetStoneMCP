﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using OpenAI;
using OpenAI.Chat;
using System.Net.Mail;
using System.Text;

string apiKey = "";
string botKey = "";
string model = "gpt-4o-mini";

IChatClient chatClient;
var messages = new List<Microsoft.Extensions.AI.ChatMessage>();
IList<McpClientTool> tools;
IClientTransport clientTransport;


var transportType = Environment.GetEnvironmentVariable("TRANSPORT_TYPE") ?? "Stdio";

if (transportType == "Stdio")
{
    clientTransport = new StdioClientTransport(new StdioClientTransportOptions
    {
        Name = "NetStoneMCP",
        Command = "dotnet",
        Arguments = ["run", "--project", "../../../../../src/NetStoneMCP.csproj", "--no-build"],
    });
}
else
{
    var sseUrl = new Uri("http://localhost:5000/sse");

    clientTransport = new SseClientTransport(new SseClientTransportOptions
    {
        Name = "NetStoneMCP",
        Endpoint = sseUrl
    });
}

var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

tools = await mcpClient.ListToolsAsync();

apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
botKey = Environment.GetEnvironmentVariable("DISCORD_BOT_KEY") ?? string.Empty;

chatClient = new OpenAIClient(apiKey).GetChatClient(model).AsIChatClient()
                    .AsBuilder().UseFunctionInvocation().Build();

// 每 5 分鐘清空一次 messages
_ = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(TimeSpan.FromMinutes(5));
        messages.Clear();
        messages.Add(new(ChatRole.System, "請使用繁體中文回答所有問題。"));
        messages.Add(new(ChatRole.System, "公司是指公會。"));
        messages.Add(new(ChatRole.System, "Link shell是指通訊貝。"));
        Console.WriteLine("[Info] 已清空 messages 並重新加入 system 指令");
    }
});

var client = new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds |
                     GatewayIntents.GuildMessages |
                     GatewayIntents.MessageContent
});

client.Log += msg =>
{
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
};

client.MessageReceived += async message =>
{
    if (message.Author.Id == client.CurrentUser.Id)
        return;

    var mentioned = message.MentionedUsers.Any(u => u.Id == client.CurrentUser.Id);

    if (mentioned == false)
    {
        return;
    }

    var imageString = string.Empty;

    var responseMessage = new StringBuilder();

    string content = message.Content
        .Replace($"<@{client.CurrentUser.Id}>", "")
        .Replace($"<@!{client.CurrentUser.Id}>", "")
        .Trim();

    var imageUrls = message.Attachments
       .Where(a => a.ContentType?.StartsWith("image/") == true)
       .Select(a => a.Url);

    if (imageUrls.Any())
    {
        content += "\n\nImage Url：\n" + string.Join("\n", imageUrls);
    }

    messages.Add(new(ChatRole.User, content));

    await message.Channel.TriggerTypingAsync();

    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
    {
        responseMessage.Append(update.Text);
    }

    messages.Add(new(ChatRole.Assistant, responseMessage.ToString()));

    await message.Channel.SendMessageAsync(responseMessage.ToString());
};

await client.LoginAsync(TokenType.Bot, botKey);
await client.StartAsync();

Console.WriteLine("Bot 已啟動，按 Ctrl+C 結束");
await Task.Delay(-1);