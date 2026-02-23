FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/LooperInsights.Api/LooperInsights.Api.csproj LooperInsights.Api/
RUN dotnet restore LooperInsights.Api/LooperInsights.Api.csproj

COPY src/LooperInsights.Api/ LooperInsights.Api/
RUN dotnet publish LooperInsights.Api/LooperInsights.Api.csproj \
    -c Release \
    -o /app/publish \
    --self-contained true 

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["./LooperInsights.Api"]
