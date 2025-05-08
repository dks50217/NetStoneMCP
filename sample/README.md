## Sample

* Claude Desktop

add mcpServers in `claude_desktop_config.json`

```json
{
  "mcpServers": {
    "NetStone MCP Server" :{
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        // NetStoneMCP.csproj path ex:
        "C:\\Users\\Desktop\\NetStoneMCP\\src\\NetStoneMCP.csproj",
        "--no-build"
      ]
    }
  }
}
```

* Custom WPF

```shell
export OPENAI_API_KEY=your_api_key_here
dotnet run --project NetStoneClient\NetStoneClient.csproj
```

* Discord Bot

```shell
export OPENAI_API_KEY=your_api_key_here
export DISCORD_BOT_KEY=your_discord_bot_token_here
dotnet run --project NetStoneDiscordBot\NetStoneDiscordBot.csproj
```

* ChatGPT Desktop

> I hope ChatGPT Desktop will support adding MCP soon.

