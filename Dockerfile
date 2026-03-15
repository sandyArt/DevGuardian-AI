# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files first (layer-cache friendly)
COPY DevGuardianAI.sln nuget.config ./
COPY src/DevGuardian.AgentRuntime/DevGuardian.AgentRuntime.csproj  src/DevGuardian.AgentRuntime/
COPY src/DevGuardian.Tools/DevGuardian.Tools.csproj                src/DevGuardian.Tools/
COPY src/DevGuardian.API/DevGuardian.API.csproj                    src/DevGuardian.API/
COPY src/DevGuardian.Tests/DevGuardian.Tests.csproj                src/DevGuardian.Tests/

RUN dotnet restore DevGuardianAI.sln --configfile nuget.config

# Copy all source code
COPY . .

# Run tests
RUN dotnet test src/DevGuardian.Tests/DevGuardian.Tests.csproj \
    --no-restore --configuration Release

# Publish API
RUN dotnet publish src/DevGuardian.API/DevGuardian.API.csproj \
    --no-restore --configuration Release \
    --output /app/publish

# ── Stage 2: Runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published artefacts
COPY --from=build /app/publish .

# Copy agent specs so they are available inside the container
COPY specs/ ./specs/

# Non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

EXPOSE 8080

# Environment variable defaults (override via docker-compose or Kubernetes secrets)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DevGuardian__SpecsPath=/app/specs

ENTRYPOINT ["dotnet", "DevGuardian.API.dll"]
