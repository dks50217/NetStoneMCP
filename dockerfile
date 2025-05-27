FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ./src/NetStoneMCP.csproj ./src/
RUN dotnet restore ./src/NetStoneMCP.csproj

COPY ./sample/NetStoneDiscordBot/NetStoneDiscordBot.csproj ./sample/NetStoneDiscordBot/
RUN dotnet restore ./sample/NetStoneDiscordBot/NetStoneDiscordBot.csproj

COPY ./src ./src
COPY ./sample ./sample

RUN dotnet publish ./src/NetStoneMCP.csproj -c Release -o /app/out/NetStoneMCP

RUN dotnet publish ./sample/NetStoneDiscordBot/NetStoneDiscordBot.csproj -c Release -o /app/out/NetStoneDiscordBot

WORKDIR /app

COPY --from=build /app/out/NetStoneMCP ./NetStoneMCP
COPY --from=build /app/out/NetStoneDiscordBot ./NetStoneDiscordBot

CMD ["sh", "-c", "dotnet ./NetStoneMCP/NetStoneMCP.dll --urls http://localhost:5000 & dotnet ./NetStoneDiscordBot/NetStoneDiscordBot.dll & wait"]