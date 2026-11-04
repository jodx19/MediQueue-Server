# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore dependencies
COPY ["MediQueue.sln", "./"]
COPY ["MediQueue.API/MediQueue.API.csproj", "MediQueue.API/"]
COPY ["MediQueue.Application/MediQueue.Application.csproj", "MediQueue.Application/"]
COPY ["MediQueue.Domain/MediQueue.Domain.csproj", "MediQueue.Domain/"]
COPY ["MediQueue.Infrastructure/MediQueue.Infrastructure.csproj", "MediQueue.Infrastructure/"]
RUN dotnet restore "MediQueue.sln"

# Copy full source code and build
COPY . .
WORKDIR "/src/MediQueue.API"
RUN dotnet publish "MediQueue.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose standard port for Render (Render maps external ports to internal 80 or 8080 by default)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MediQueue.API.dll"]
