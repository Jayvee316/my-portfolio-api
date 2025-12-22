# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY MyPortfolio.Api.sln ./
COPY MyPortfolio.Api/MyPortfolio.Api.csproj MyPortfolio.Api/
COPY MyPortfolio.Core/MyPortfolio.Core.csproj MyPortfolio.Core/
COPY MyPortfolio.Infrastructure/MyPortfolio.Infrastructure.csproj MyPortfolio.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/MyPortfolio.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port (Render uses PORT env variable)
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MyPortfolio.Api.dll"]
