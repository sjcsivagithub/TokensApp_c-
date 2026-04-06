# ──────────────────────────────────────────────────────────────────────────────
# Stage 1: BUILD
#   Uses the full .NET SDK to restore dependencies and publish in Release mode.
#   Layer-caching: csproj + restore runs first so it only re-runs when deps change.
# ──────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore (cached layer)
COPY ["TokensApp.csproj", "."]
RUN dotnet restore

# Copy remaining source and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ──────────────────────────────────────────────────────────────────────────────
# Stage 2: RUNTIME
#   Uses the smaller ASP.NET runtime image (no SDK bloat).
#   Render injects $PORT at runtime; ASPNETCORE_URLS picks it up automatically.
# ──────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render.com dynamic port — defaults to 10000 if PORT is not set
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}

# Document the port Render expects (Render free tier uses 10000 by default)
EXPOSE 10000

ENTRYPOINT ["dotnet", "TokensApp.dll"]
