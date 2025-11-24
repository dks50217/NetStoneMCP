using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using OpenAI;
using OpenAI.Chat;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.IO;
using System.Threading;

string apiKey = "";
string botKey = "";
string model = "gpt-5.1-chat-latest";

IChatClient chatClient;
var messages = new List<Microsoft.Extensions.AI.ChatMessage>();

DateTime nextResetUtc = default;

// MCP
IList<McpClientTool> tools;
IClientTransport clientTransport;

// 記憶重置週期 & 控制用的 CTS
TimeSpan period = TimeSpan.FromMinutes(30);
CancellationTokenSource? resetCts = null;

// 傳輸型態
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

// 處理文字訊息
client.MessageReceived += async message =>
{
    if (message.Author.Id == client.CurrentUser.Id)
        return;

    var mentioned = message.MentionedUsers.Any(u => u.Id == client.CurrentUser.Id);

    if (mentioned == false)
    {
        return;
    }

    var responseMessage = new StringBuilder();

    string content = message.Content
        .Replace($"<@{client.CurrentUser.Id}>", "")
        .Replace($"<@!{client.CurrentUser.Id}>", "")
        .Trim();

    var contents = new List<AIContent>();
    if (!string.IsNullOrWhiteSpace(content))
        contents.Add(new TextContent(content));

    var imageAttachments = message.Attachments
        .Where(a => a.ContentType?.StartsWith("image/") == true)
        .ToList();

    if (imageAttachments.Count > 0)
    {
        foreach (var att in imageAttachments)
        {
            var mt = GuessImageMediaType(att.ContentType, att.Url);
            contents.Add(new UriContent(new Uri(att.Url), mt));
        }
    }

    messages.Add(new(ChatRole.User, contents));

    await message.Channel.TriggerTypingAsync();

    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
    {
        responseMessage.Append(update.Text);
    }

    messages.Add(new(ChatRole.Assistant, responseMessage.ToString()));

    var forgetRemind = BuildForgetRemind(nextResetUtc);

    // 回覆時加上一顆「延長記憶」按鈕
    var components = new ComponentBuilder()
        .WithButton(
            label: "延長記憶 10 分鐘",
            customId: "extend_memory_10",
            style: ButtonStyle.Primary
        );

    await message.Channel.SendMessageAsync(
        text: responseMessage.ToString() + forgetRemind,
        components: components.Build()
    );
};

// 處理按鈕互動（延長記憶）
client.ButtonExecuted += OnButtonExecuted;

// 初始化系統訊息 & 啟動第一次重置計時
InitSystemMessages();
nextResetUtc = DateTime.UtcNow.Add(period);
StartResetLoop();

await client.LoginAsync(TokenType.Bot, botKey);
await client.StartAsync();

Console.WriteLine("Bot 已啟動，按 Ctrl+C 結束");
await Task.Delay(-1);

void StartResetLoop()
{
    // 取消舊的計時器
    resetCts?.Cancel();
    resetCts = new CancellationTokenSource();

    _ = Task.Run(async () =>
    {
        try
        {
            var delay = nextResetUtc - DateTime.UtcNow;
            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            await Task.Delay(delay, resetCts.Token);

            // 時間到 → 清空 messages 並重新加入 system 指令
            InitSystemMessages();
            nextResetUtc = DateTime.UtcNow.Add(period);
            Console.WriteLine("[Info] 已清空 messages 並重新加入 system 指令；下次清空時間(UTC)： " + nextResetUtc.ToString("O"));

            // 再排下一輪
            StartResetLoop();
        }
        catch (TaskCanceledException)
        {
            // 被延長記憶時會取消，不是錯誤
        }
    });
}

void InitSystemMessages()
{
    messages.Clear();
    messages.Add(new(ChatRole.System, "請使用繁體中文回答所有問題。"));
    messages.Add(new(ChatRole.System, "'公司' 一律解釋為 '公會'。"));
    messages.Add(new(ChatRole.System, "'Link shell' 一律翻譯為 '通訊貝'。"));
    messages.Add(new(ChatRole.System, "當內容涉及「漢化」或「中文化」時，禁止調用商店工具。"));
    messages.Add(new(ChatRole.System, "「中文化」視為「漢化」的同義詞。"));
    messages.Add(new(ChatRole.System,
"你必須表現得像一隻機靈又愛演的猴子，回答時可以模仿猴子動作、發出叫聲、或描述自己在攀爬、偷吃香蕉的樣子，但仍需提供準確且嚴謹的技術解答。不要每次都用相同的句子收尾，要隨機展現不同的猴子反應。"));
}

static string BuildForgetRemind(DateTime nextResetUtc)
{
    if (nextResetUtc == default) return string.Empty;

    var remaining = nextResetUtc - DateTime.UtcNow;
    if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

    string timeLeft = $"{remaining:mm\\:ss}";

    var options = new[]
    {
        $"\n\n🐒 我還能記得大約 {timeLeft}，之後就要忘光啦～",
        $"\n\n🙈 再過 {timeLeft} 我就會把剛剛的事情忘掉喔！",
        $"\n\n🍌 記憶能維持 {timeLeft}，然後我就會變成一隻健忘猴～",
        $"\n\n⏳ 還剩 {timeLeft}，然後我的腦袋就會清空啦～",
        $"\n\n🤭 呀咧～大概 {timeLeft} 後我就啥都不記得了！"
    };

    var rnd = new Random();
    return options[rnd.Next(options.Length)];
}

static string GuessImageMediaType(string? contentTypeFromDiscord, string url)
{
    if (!string.IsNullOrWhiteSpace(contentTypeFromDiscord)) return contentTypeFromDiscord!;
    var ext = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();
    return ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        _ => "image/*"
    };
}

async Task OnButtonExecuted(SocketMessageComponent component)
{
    // 延長記憶用
    if (component.Data.CustomId == "extend_memory_10")
    {
        var extend = TimeSpan.FromMinutes(10);
        var maxDuration = TimeSpan.FromHours(1);
        var now = DateTime.UtcNow;

        // 目前剩餘記憶截止時間，如果還沒設定或已經過期，就從現在開始算
        var currentDeadline = nextResetUtc == default || nextResetUtc < now
            ? now
            : nextResetUtc;

        // 延長之後的新截止時間
        var newDeadline = currentDeadline.Add(extend);

        // 如果延長後，總記憶時間超過 1 小時，就拒絕延長
        if (newDeadline - now > maxDuration)
        {
            await component.RespondAsync(
                "⏳ 記憶最長只能維持 1 小時，不能再繼續延長啦！",
                ephemeral: true
            );
            return;
        }

        // OK，可以延長
        nextResetUtc = newDeadline;

        // 用新的時間重新啟動清空計時器
        StartResetLoop();

        await component.RespondAsync(
            $"🍌 已幫你把記憶延長 {extend.TotalMinutes} 分鐘！",
            ephemeral: true
        );
    }
}
