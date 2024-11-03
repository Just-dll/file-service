# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FileService.BFF/FileService.BFF.csproj", "FileSerivce.BFF/"]
COPY ["FileSerivce.ServiceDefaults/FileSerivce.ServiceDefaults.csproj", "FileSerivce.ServiceDefaults/"]
WORKDIR "/src/FileSerivce.BFF/."
RUN dotnet restore "./FileService.BFF.csproj"
WORKDIR "/src/."
COPY . .
WORKDIR "/src/FileService.BFF/."
RUN dotnet build "./FileService.BFF.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.BFF.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.BFF.dll"]