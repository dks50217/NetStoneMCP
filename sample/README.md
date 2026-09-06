## Samples & Client Configuration

### Claude Desktop (Stdio Mode)

Add the server to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "netstone-mcp": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\NetStoneMCP\\src\\NetStoneMCP.csproj",
        "--no-build"
      ]
    }
  }
}
```

### HTTP / SSE Mode (Cursor, VS Code, or Web Clients)

Start NetStoneMCP in SSE transport mode:

```shell
export TransportType=SSE
dotnet run --project src/NetStoneMCP.csproj
```

The MCP SSE endpoint will be available at:
`http://localhost:5000/sse`

### Custom WPF Client

```shell
export OPENAI_API_KEY=your_api_key_here
dotnet run --project sample/NetStoneClient/NetStoneClient.csproj
```

### Discord Bot

```shell
export OPENAI_API_KEY=your_api_key_here
export DISCORD_BOT_KEY=your_discord_bot_token_here
# Optional: Transport mode ("Stdio" or "SSE")
export TRANSPORT_TYPE=SSE
dotnet run --project sample/NetStoneDiscordBot/NetStoneDiscordBot.csproj
```

### Docker Compose

See `.env.example` to configure your environment, then run:

```shell
docker compose up -d --build
```


