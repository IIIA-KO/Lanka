FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 4308

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Api/Lanka.Api/Lanka.Api.csproj", "src/Api/Lanka.Api/"]
COPY ["src/Api/Lanka.Api/localhost.pfx", "src/Api/Lanka.Api/"]
RUN dotnet restore "src/Api/Lanka.Api/Lanka.Api.csproj"
COPY . .
WORKDIR "/src/src/Api/Lanka.Api"
RUN dotnet build "Lanka.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Lanka.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["src/Api/Lanka.Api/localhost.pfx", "/app/"]
ENTRYPOINT ["dotnet", "Lanka.Api.dll"]
