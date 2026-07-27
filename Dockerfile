# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution files
COPY ["MediQueue.sln", "."]
COPY ["MediQueue.API/MediQueue.API.csproj", "MediQueue.API/"]
COPY ["MediQueue.Application/MediQueue.Application.csproj", "MediQueue.Application/"]
COPY ["MediQueue.Domain/MediQueue.Domain.csproj", "MediQueue.Domain/"]
COPY ["MediQueue.Infrastructure/MediQueue.Infrastructure.csproj", "MediQueue.Infrastructure/"]
COPY ["MediQueue.Tests/MediQueue.Tests.csproj", "MediQueue.Tests/"]

# Restore
RUN dotnet restore "MediQueue.API/MediQueue.API.csproj"

# Copy all source
COPY . .

# Build
WORKDIR "/src/MediQueue.API"
RUN dotnet build "MediQueue.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MediQueue.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Security: non-root user
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD wget -qO- http://localhost:8080/health/live || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MediQueue.API.dll"]
