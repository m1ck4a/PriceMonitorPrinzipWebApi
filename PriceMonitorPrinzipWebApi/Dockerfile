# STAGE 1: Runtime base (Debian Bookworm + .NET 10)
# Официальный образ с поддержкой non-root пользователя 'app'
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# STAGE 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Кэшируем restore — ускоряет повторные сборки
COPY ["PriceMonitorPrinzipWebApi.csproj", "."]
RUN dotnet restore "PriceMonitorPrinzipWebApi.csproj"

# Копируем исходный код
COPY . .
WORKDIR "/src"
RUN dotnet build "PriceMonitorPrinzipWebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# STAGE 3: Publish (оптимизированная публикация)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PriceMonitorPrinzipWebApi.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishAot=false \
    /p:InvariantGlobalization=true

# STAGE 4: Final runtime image
FROM base AS final
WORKDIR /app

# Копируем только необходимые файлы
COPY --from=publish /app/publish .

# Health check для мониторинга живости
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PriceMonitorPrinzipWebApi.dll"]