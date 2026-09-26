# TripCraft API — multi-stage build: .NET 8 SDK compiles, the small aspnet runtime image runs it.
# Build context is backend/:  docker build -t tripcraft-api backend

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first, from the project files only, so this layer is cached until a dependency changes.
COPY src/TripCraft.Api/TripCraft.Api.csproj src/TripCraft.Api/
COPY src/TripCraft.Application/TripCraft.Application.csproj src/TripCraft.Application/
COPY src/TripCraft.Infrastructure/TripCraft.Infrastructure.csproj src/TripCraft.Infrastructure/
RUN dotnet restore src/TripCraft.Api/TripCraft.Api.csproj

COPY src/ src/
RUN dotnet publish src/TripCraft.Api/TripCraft.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    UPLOADS_DIR=/app/uploads

COPY --from=build /app .

# Run as the image's built-in non-root user ("app", uid 1654). Only the uploads folder is writable.
RUN mkdir -p /app/uploads && chown -R $APP_UID /app/uploads
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "TripCraft.Api.dll"]
