FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["portal-por-elas.Gateway/portal-por-elas.Gateway.csproj", "portal-por-elas.Gateway/"]
RUN dotnet restore "portal-por-elas.Gateway/portal-por-elas.Gateway.csproj"
COPY . .
WORKDIR "/src/portal-por-elas.Gateway"
RUN dotnet publish "portal-por-elas.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "portal-por-elas.Gateway.dll"]