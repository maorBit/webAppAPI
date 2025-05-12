# Use .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EmailWebApp/EmailWebApp.csproj", "EmailWebApp/"]
RUN dotnet restore "EmailWebApp/EmailWebApp.csproj"
COPY . .
WORKDIR "/src/EmailWebApp"
RUN dotnet build "EmailWebApp.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "EmailWebApp.csproj" -c Release -o /app/publish

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailWebApp.dll"]
