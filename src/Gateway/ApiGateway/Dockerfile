# ============================================================
# STAGE 1: Build
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Gateway/ApiGateway/ApiGateway.csproj", "Gateway/ApiGateway/"]

RUN dotnet restore "Gateway/ApiGateway/ApiGateway.csproj"

COPY src/ .

RUN dotnet publish "Gateway/ApiGateway/ApiGateway.csproj" \
    -c Release -o /app/publish --no-restore

# ============================================================
# STAGE 2: Runtime
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiGateway.dll"]