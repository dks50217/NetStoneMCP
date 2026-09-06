# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```sh
# Build the MCP server
dotnet build src/NetStoneMCP.csproj

# Run the MCP server (SSE mode by default per appsettings.json)
dotnet run --project src/NetStoneMCP.csproj

# Run all tests
dotnet test tests/NetStoneMCP.Tests/NetStoneMCP.Tests.csproj

# Run a single test by name
dotnet test tests/NetStoneMCP.Tests/NetStoneMCP.Tests.csproj --filter "FullyQualifiedName~GetCharacterId_WithoutInitialize_Throws"

# Run the Discord bot sample
dotnet run --project sample/NetStoneDiscordBot/NetStoneDiscordBot.csproj

# Run the WPF client sample (Windows only)
dotnet run --project sample/NetStoneClient/NetStoneClient.csproj
```

## Architecture

This is a .NET 8 MCP (Model Context Protocol) server that exposes FFXIV Lodestone data as LLM-callable tools. The solution file is at `src/NetStoneMCP.sln`.

### Transport

Controlled by `TransportType` in `src/appsettings.json`. `"SSE"` runs as an HTTP server (`app.MapMcp()`); anything else (or omitted) uses stdio transport for direct MCP client integration (e.g., Claude Desktop).

### Tool/Service pattern

Every feature follows the same pattern:
- **`src/Tools/`** — Classes decorated with `[McpServerToolType]`. Each public method decorated with `[McpServerTool]` becomes an MCP-callable tool. Tools receive services via constructor injection and return DTOs.
- **`src/Services/`** — Interface + implementation pairs registered in `Program.cs`. Services do the actual HTTP calls or library calls. `NetStoneService` is a singleton wrapping the `LodestoneClient` (must be initialized via `InitializeAsync()` on startup). All other HTTP services use `AddHttpClient<>`.
- **`src/Model/`** — DTO record/class types returned by tools.
- **`src/Dict/`** — Static lookup dictionaries (e.g., race names, housing data).
- **`src/Const/`** — Enum-like constant classes.

### Services and their data sources

| Service | Data Source |
|---|---|
| `NetStoneService` | NetStone library → FFXIV Lodestone (characters, FC, linkshells) |
| `CommonService` | PaissaDB API (worlds/datacenters), ffxiv.com.tw (CHT server status) |
| `PaissaHouseService` | FFXIV_PaissaHouse API |
| `StoreService` | FFXIV Store |
| `XIVAPIService` | XIVAPI (item data) |
| `LodeStoneNewsService` | lodestonenews.com |
| `ThaliakService` | thaliak.xiv.dev (game versions) |
| `NoteService` | Local in-memory storage |
| `CustomService` | GitHub API via Octokit (CHT patch releases) |
| `FFXIVCollectService` | ffxivcollect.com (mounts, minions, emotes, hairstyles, spells, etc.) |

### Adding a new tool

1. Create a service interface + class in `src/Services/`, register it in `Program.cs`.
2. Create a tool class in `src/Tools/` with `[McpServerToolType]` on the class and `[McpServerTool(Name = "...", Title = "...")]` + `[Description("...")]` on each method.
3. Tools are auto-discovered via `WithToolsFromAssembly()` — no manual registration needed.

### Tests

Tests use NUnit 3 and are in `tests/NetStoneMCP.Tests/Services/`. `FakeHttpMessageHandler` in `tests/NetStoneMCP.Tests/Helpers/` is used to mock HTTP responses.

### Samples

- `sample/NetStoneDiscordBot/` — Discord bot using OpenAI function calling against the MCP tools. Requires `OPENAI_API_KEY` and `DISCORD_BOT_KEY` env vars.
- `sample/NetStoneClient/` — WPF GUI client. Requires `OPENAI_API_KEY`.
- Docker: `docker build -f ./dockerfile -t netstone-mcp .` / `docker compose up -d` (needs `.env` with `OPENAI_API_KEY` and `DISCORD_BOT_KEY`).
