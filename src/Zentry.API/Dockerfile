﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Đồng bộ UID/GID với host (được truyền từ docker-compose)
ARG APP_UID=1000
ARG APP_GID=1000

# Tạo user non-root với UID từ host (JetBrains mount vào với UID host)
RUN addgroup --gid $APP_GID appgroup && \
    adduser --disabled-password --gecos "" --uid $APP_UID --gid $APP_GID appuser

USER appuser
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Zentry.API/Zentry.API.csproj", "src/Zentry.API/"]
RUN dotnet restore "src/Zentry.API/Zentry.API.csproj"
COPY . .
WORKDIR "/src/src/Zentry.API"
RUN dotnet build "./Zentry.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Zentry.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zentry.API.dll"]
