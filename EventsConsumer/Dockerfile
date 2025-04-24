FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EventsConsumer/EventsConsumer.csproj", "EventsConsumer/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "EventsConsumer/EventsConsumer.csproj"
COPY . .
WORKDIR "/src/EventsConsumer"
RUN dotnet build "EventsConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventsConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsConsumer.dll"]
