# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AgentForge.Orchestrator.csproj", "."]
RUN dotnet restore "./AgentForge.Orchestrator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./AgentForge.Orchestrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AgentForge.Orchestrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

# Install curl for the container healthcheck. The base stage already switched to
# the non-root app user, so flip back to root for the package install (apt needs
# root — running it as the app user is what made the CI build fail), then return.
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID

COPY --from=publish /app/publish .

# Self-contained healthcheck so the image is healthy on any compose/orchestrator.
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=10 \
  CMD curl --fail --silent http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "AgentForge.Orchestrator.dll"]