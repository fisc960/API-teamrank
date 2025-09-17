# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore (speeds up rebuilds)
COPY ["GemachApp.csproj", "./"]
RUN dotnet restore "./GemachApp.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "GemachApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Let Program.cs read PORT env var at runtime
ENTRYPOINT ["dotnet", "GemachApp.dll"]