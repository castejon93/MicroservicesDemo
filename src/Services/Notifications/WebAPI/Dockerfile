# ============================================================
# STAGE 1: Build
# Use the .NET 10 SDK image to restore, build and publish.
# Copying .csproj files first lets Docker cache the restore
# layer — it only re-runs dotnet restore when a .csproj changes.
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for restore (better layer caching)
COPY ["src/Services/Notifications/WebAPI/Notifications.WebAPI.csproj",                 "Services/Notifications/WebAPI/"]
COPY ["src/Services/Notifications/Application/Notifications.Application.csproj",       "Services/Notifications/Application/"]
COPY ["src/Services/Notifications/Domain/Notifications.Domain.csproj",                 "Services/Notifications/Domain/"]
COPY ["src/Services/Notifications/Infrastructure/Notifications.Infrastructure.csproj", "Services/Notifications/Infrastructure/"]
COPY ["src/SharedKernel/SharedKernel.csproj",                                           "SharedKernel/"]

# Restore NuGet packages (cached unless a .csproj changed)
RUN dotnet restore "Services/Notifications/WebAPI/Notifications.WebAPI.csproj"

# Copy remaining source files and publish
COPY src/ .

RUN dotnet publish "Services/Notifications/WebAPI/Notifications.WebAPI.csproj" \
    -c Release -o /app/publish --no-restore

# ============================================================
# STAGE 2: Runtime
# Use the smaller ASP.NET runtime image — no SDK overhead.
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# .NET 10 listens on 8080 by default inside the container.
# docker-compose maps host port 5003 → container port 8080.
EXPOSE 8080

ENTRYPOINT ["dotnet", "Notifications.WebAPI.dll"]
