﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Zentry.BackgroundProcessor/Zentry.BackgroundProcessor.csproj", "src/Zentry.BackgroundProcessor/"]
COPY ["src/Zentry.Infrastructure/Zentry.Infrastructure.csproj", "src/Zentry.Infrastructure/"]
RUN dotnet restore "src/Zentry.BackgroundProcessor/Zentry.BackgroundProcessor.csproj"
COPY . .
WORKDIR "/src/src/Zentry.BackgroundProcessor"
RUN dotnet build "./Zentry.BackgroundProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Zentry.BackgroundProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Zentry.BackgroundProcessor.dll"]
