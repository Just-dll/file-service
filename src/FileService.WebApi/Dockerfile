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
COPY ["FileService.WebApi/FileService.WebApi.csproj", "FileSerivce.WebApi/"]
COPY ["FileService.DAL/FileService.DAL.csproj", "FileService.DAL/"]
COPY ["FileService.BLL/FileService.BLL.csproj", "FileService.BLL/"]
COPY ["FileSerivce.ServiceDefaults/FileSerivce.ServiceDefaults.csproj", "FileSerivce.ServiceDefaults/"]
WORKDIR "/src/FileSerivce.WebApi/."
RUN dotnet restore "./FileService.WebApi.csproj"
WORKDIR "/src/."
COPY . .
WORKDIR "/src/FileService.WebApi/."
RUN dotnet build "./FileService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.WebApi.dll"]