# ============================================================
# STAGE 1: Build
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Products/WebAPI/Products.WebAPI.csproj",                 "Services/Products/WebAPI/"]
COPY ["src/Services/Products/Application/Products.Application.csproj",       "Services/Products/Application/"]
COPY ["src/Services/Products/Domain/Products.Domain.csproj",                 "Services/Products/Domain/"]
COPY ["src/Services/Products/Infrastructure/Products.Infrastructure.csproj", "Services/Products/Infrastructure/"]
COPY ["src/SharedKernel/SharedKernel.csproj",                                "SharedKernel/"]

RUN dotnet restore "Services/Products/WebAPI/Products.WebAPI.csproj"

COPY src/ .

RUN dotnet publish "Services/Products/WebAPI/Products.WebAPI.csproj" \
    -c Release -o /app/publish --no-restore

# ============================================================
# STAGE 2: Runtime
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Products.WebAPI.dll"]