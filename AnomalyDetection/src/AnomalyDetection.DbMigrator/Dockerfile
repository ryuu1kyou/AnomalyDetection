FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/AnomalyDetection.DbMigrator/AnomalyDetection.DbMigrator.csproj", "src/AnomalyDetection.DbMigrator/"]
COPY ["src/AnomalyDetection.EntityFrameworkCore/AnomalyDetection.EntityFrameworkCore.csproj", "src/AnomalyDetection.EntityFrameworkCore/"]
COPY ["src/AnomalyDetection.Domain/AnomalyDetection.Domain.csproj", "src/AnomalyDetection.Domain/"]
COPY ["src/AnomalyDetection.Domain.Shared/AnomalyDetection.Domain.Shared.csproj", "src/AnomalyDetection.Domain.Shared/"]

# Restore dependencies
RUN dotnet restore "src/AnomalyDetection.DbMigrator/AnomalyDetection.DbMigrator.csproj"

# Copy source code
COPY . .

# Build and publish
WORKDIR "/src/src/AnomalyDetection.DbMigrator"
RUN dotnet build "AnomalyDetection.DbMigrator.csproj" -c Release -o /app/build
RUN dotnet publish "AnomalyDetection.DbMigrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AnomalyDetection.DbMigrator.dll"]