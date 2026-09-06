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

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/out/NetStoneMCP ./NetStoneMCP
COPY --from=build /app/out/NetStoneDiscordBot ./NetStoneDiscordBot

ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV TransportType=SSE
ENV TRANSPORT_TYPE=SSE
ENV NETSTONE_SSE_URL=http://localhost:5000/sse

EXPOSE 5000

CMD ["sh", "-c", "dotnet ./NetStoneMCP/NetStoneMCP.dll --urls http://0.0.0.0:5000 & sleep 2 && dotnet ./NetStoneDiscordBot/NetStoneDiscordBot.dll & wait"]