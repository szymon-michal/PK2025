# Use the official .NET 8.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 8.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ConnectionPlatform.API/ConnectionPlatform.API.csproj", "ConnectionPlatform.API/"]
COPY ["ConnectionPlatform.Core/ConnectionPlatform.Core.csproj", "ConnectionPlatform.Core/"]
COPY ["ConnectionPlatform.Infrastructure/ConnectionPlatform.Infrastructure.csproj", "ConnectionPlatform.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ConnectionPlatform.API/ConnectionPlatform.API.csproj"

# Copy the source code
COPY . .

# Build the application
WORKDIR "/src/ConnectionPlatform.API"
RUN dotnet build "ConnectionPlatform.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
WORKDIR /src/ConnectionPlatform.API
RUN dotnet publish "ConnectionPlatform.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ConnectionPlatform.API.dll"]