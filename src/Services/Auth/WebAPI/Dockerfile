# ============================================================
# STAGE 1: Build
# Use the .NET 10 SDK image to restore, build and publish
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project files first (better Docker layer caching)
COPY ["src/Services/Auth/WebAPI/Auth.WebAPI.csproj",           "Services/Auth/WebAPI/"]
COPY ["src/Services/Auth/Application/Auth.Application.csproj", "Services/Auth/Application/"]
COPY ["src/Services/Auth/Domain/Auth.Domain.csproj",           "Services/Auth/Domain/"]
COPY ["src/Services/Auth/Infrastructure/Auth.Infrastructure.csproj", "Services/Auth/Infrastructure/"]

# Restore NuGet packages
RUN dotnet restore "Services/Auth/WebAPI/Auth.WebAPI.csproj"

# Copy all remaining source files
COPY src/ .

# Publish in Release mode to /app/publish
RUN dotnet publish "Services/Auth/WebAPI/Auth.WebAPI.csproj" \
    -c Release -o /app/publish --no-restore

# ============================================================
# STAGE 2: Runtime
# Use the smaller ASP.NET runtime image (no SDK)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# .NET 10 default port inside container
EXPOSE 8080

ENTRYPOINT ["dotnet", "Auth.WebAPI.dll"]