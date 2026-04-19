ARG DOTNET_RUNTIME_VERSION=10.0.1
ARG DOTNET_SDK_VERSION=10.0.101

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_RUNTIME_VERSION} AS base
USER app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
RUN GRPC_HEALTH_PROBE_VERSION=v0.4.13 && \
    wget -O /bin/grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/${GRPC_HEALTH_PROBE_VERSION}/grpc_health_probe-linux-amd64 && \
    chmod +x /bin/grpc_health_probe
WORKDIR /src
COPY ["MarketBasketAnalysisStorage.Api/MarketBasketAnalysisStorage.Api.csproj", "MarketBasketAnalysisStorage.Api/"]
RUN dotnet restore "./MarketBasketAnalysisStorage.Api/MarketBasketAnalysisStorage.Api.csproj"
COPY . .
WORKDIR "/src/MarketBasketAnalysisStorage.Api"
RUN dotnet build "./MarketBasketAnalysisStorage.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MarketBasketAnalysisStorage.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /bin/grpc_health_probe /bin
ENTRYPOINT dotnet MarketBasketAnalysis.Server.API.dll