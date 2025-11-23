FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 4308

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Api/Lanka.Gateway/Lanka.Gateway.csproj", "src/Api/Lanka.Gateway/"]
COPY ["src/Api/Lanka.Gateway/localhost.pfx", "src/Api/Gateway.Api/"]
RUN dotnet restore "src/Api/Lanka.Gateway/Lanka.Gateway.csproj"
COPY . .
WORKDIR "/src/src/Api/Lanka.Gateway"
RUN dotnet build "Lanka.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lanka.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["src/Api/Lanka.Gateway/localhost.pfx", "/app/"]
ENTRYPOINT ["dotnet", "Lanka.Gateway.dll"]
