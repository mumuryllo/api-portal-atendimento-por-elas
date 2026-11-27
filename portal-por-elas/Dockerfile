FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["portal-por-elas/portal-por-elas.csproj", "portal-por-elas/"]
RUN dotnet restore "portal-por-elas/portal-por-elas.csproj"
COPY . .
WORKDIR "/src/portal-por-elas"
RUN dotnet publish "portal-por-elas.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "portal-por-elas.dll"]