# NetStone.MCP

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/dks50217/NetStoneMCP)

MCP Toolset for FFXIV Lodestone — Integrates [NetStone](https://github.com/xivapi/NetStone) into a natural-language-capable MCP server for querying character and world data from Final Fantasy XIV.

![[Not By AI](https://notbyai.fyi/hi/not-by-ai/)](./docs/Produced-By-Human-Not-By-AI-Badge-white.png)

## Overview

This project transforms FFXIV Lodestone API library NetStone, along with other external APIs, into a set of Model Context Protocol (MCP) tools. It enables users to query Lodestone and related game data using natural language prompts through any MCP-compatible LLM client such as OpenAI or Claude.


## Data Source

* [NetStone](https://github.com/xivapi/NetStone)
* [FFXIV_PaissaHouse](https://github.com/zhudotexe/FFXIV_PaissaHouse)
* [FFXIVStore](https://store.finalfantasyxiv.com/ffxivstore/en-us/)
* [XIVAPI](https://github.com/xivapi)

## Setup

[Setup](./sample/README.md)

## Sample

* Claude Desktop

![sample5](./docs/sample5.png)

<br>

* Custom Console

<br>

![sample](./docs/sample.png)

<br>

* Custom WPF

<br>

![sample3](./docs/sample3.png)

<br>

![sample4](./docs/sample4.png)

## Features

- [x] Character search — Search for FFXIV characters by name and world.
- [x] Character profile details — Fetch detailed character profiles.
- [x] Free Company search — Search for Free Companies by name and server.
- [x] Free Company profile details — Retrieve Free Company members and data.
- [x] World list — List all supported FFXIV worlds.
- [x] House list — List all purchasable houses.
- [x] Store list — List store categories. 
- [x] search for products by specifying a category name.

## Quick Setup - Docker for Discord Bot

1. Create a `.env` file in the project root and add your keys:

    ```env
    OPENAI_API_KEY=your_openai_api_key
    DISCORD_BOT_KEY=your_discord_bot_token
    ```

2. Build the Docker image:

    ```sh
    docker build -f ./dockerfile -t netstone-mcp .
    ```

3. Start the container with Docker Compose

    ```sh
    docker compose up -d
    ```

> Make sure the `.env` file is in the same directory as your `docker-compose.yml`.



