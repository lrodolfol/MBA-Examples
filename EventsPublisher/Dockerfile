FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EventsPublisher/EventsPublisher.csproj", "EventsPublisher/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "EventsPublisher/EventsPublisher.csproj"
COPY . .
WORKDIR "/src/EventsPublisher"
RUN dotnet build "EventsPublisher.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventsPublisher.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsPublisher.dll"]
